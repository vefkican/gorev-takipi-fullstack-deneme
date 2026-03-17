using Microsoft.Extensions.Caching.Memory;
using TaskManagerAPI.Models.DTOs;
using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Repositories
{
    public class CachedTaskRepository : ITaskRepository
    {
        private readonly ITaskRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        // Hangi cache key'lerin hangi kullanıcıya ait olduğunu takip et
        private static readonly Dictionary<int, List<string>> _userCacheKeys = new();

        public CachedTaskRepository(ITaskRepository inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        private string GetListCacheKey(int userId, string? search, bool? isCompleted, int page, int pageSize, string? sortBy)
            => $"tasks_list_{userId}_{search}_{isCompleted}_{page}_{pageSize}_{sortBy}";

        private string GetByIdCacheKey(int id, int userId)
            => $"tasks_item_{userId}_{id}";

        private void TrackCacheKey(int userId, string key)
        {
            if (!_userCacheKeys.ContainsKey(userId))
                _userCacheKeys[userId] = new List<string>();
            _userCacheKeys[userId].Add(key);
        }

        private void InvalidateCache(int userId)
        {
            if (_userCacheKeys.TryGetValue(userId, out var keys))
            {
                foreach (var key in keys)
                    _cache.Remove(key);
                keys.Clear();
                Console.WriteLine($"Cache invalidated for userId: {userId}");
            }
        }

        public async Task<PagedResult<TaskItem>> GetAllAsync(int userId, string? search, bool? isCompleted, int page, int pageSize, string? sortBy = null)
        {
            var cacheKey = GetListCacheKey(userId, search, isCompleted, page, pageSize, sortBy);

            if (_cache.TryGetValue(cacheKey, out PagedResult<TaskItem>? cached))
            {
                Console.WriteLine($"Cache HIT: {cacheKey}");
                return cached!;
            }

            Console.WriteLine($"Cache MISS: {cacheKey}");
            var result = await _inner.GetAllAsync(userId, search, isCompleted, page, pageSize, sortBy);
            _cache.Set(cacheKey, result, _cacheDuration);
            TrackCacheKey(userId, cacheKey);
            return result;
        }

        public async Task<TaskItem?> GetByIdAsync(int id, int userId)
        {
            var cacheKey = GetByIdCacheKey(id, userId);

            if (_cache.TryGetValue(cacheKey, out TaskItem? cached))
            {
                Console.WriteLine($"Cache HIT: {cacheKey}");
                return cached;
            }

            Console.WriteLine($"Cache MISS: {cacheKey}");
            var result = await _inner.GetByIdAsync(id, userId);
            if (result != null)
            {
                _cache.Set(cacheKey, result, _cacheDuration);
                TrackCacheKey(userId, cacheKey);
            }
            return result;
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            var result = await _inner.CreateAsync(task);
            InvalidateCache(task.UserId);
            return result;
        }

        public async Task UpdateAsync(TaskItem task)
        {
            await _inner.UpdateAsync(task);
            InvalidateCache(task.UserId);
        }

        public async Task DeleteAsync(TaskItem task)
        {
            await _inner.DeleteAsync(task);
            InvalidateCache(task.UserId);
        }
    }
}