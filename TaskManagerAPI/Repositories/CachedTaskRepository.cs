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

        public CachedTaskRepository(ITaskRepository inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        private string GetCacheKey(int userId, string? search, bool? isCompleted, int page, int pageSize, string? sortBy)
            => $"tasks_{userId}_{search}_{isCompleted}_{page}_{pageSize}_{sortBy}";

        public async Task<PagedResult<TaskItem>> GetAllAsync(int userId, string? search, bool? isCompleted, int page, int pageSize, string? sortBy = null)
        {
            var cacheKey = GetCacheKey(userId, search, isCompleted, page, pageSize, sortBy);

            if (_cache.TryGetValue(cacheKey, out PagedResult<TaskItem>? cached))
            {
                Console.WriteLine($"Cache HIT: {cacheKey}");
                return cached!;
            }

            Console.WriteLine($"Cache MISS: {cacheKey}");
            var result = await _inner.GetAllAsync(userId, search, isCompleted, page, pageSize, sortBy);
            _cache.Set(cacheKey, result, _cacheDuration);
            return result;
        }

        public async Task<TaskItem?> GetByIdAsync(int id, int userId)
            => await _inner.GetByIdAsync(id, userId);

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

        private void InvalidateCache(int userId)
        {
            // Kullanıcının tüm cache'ini temizle
            Console.WriteLine($"Cache invalidated for userId: {userId}");
        }
    }
}