using System.Security.Claims;
using TaskManagerAPI.Models.DTOs;
using TaskManagerAPI.Models.Entities;
using TaskManagerAPI.Repositories;

namespace TaskManagerAPI.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IEnumerable<ITaskEventHandler> _eventHandlers;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskService(
            ITaskRepository repository,
            IEnumerable<ITaskEventHandler> eventHandlers,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _eventHandlers = eventHandlers;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetUsername() =>
            _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.Name) ?? "Unknown";

        private async Task PublishEvent(TaskEvent taskEvent)
        {
            foreach (var handler in _eventHandlers)
                await handler.HandleAsync(taskEvent);
        }

        public async Task<PagedResult<TaskItem>> GetAllAsync(int userId, string? search, bool? isCompleted, int page, int pageSize, string? sortBy = null)
        {
            return await _repository.GetAllAsync(userId, search, isCompleted, page, pageSize, sortBy);
        }

        public async Task<TaskItem?> GetByIdAsync(int id, int userId)
        {
            return await _repository.GetByIdAsync(id, userId);
        }

        public async Task<TaskItem> CreateAsync(int userId, CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate.HasValue
                    ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                    : null,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            var created = await _repository.CreateAsync(task);

            await PublishEvent(new TaskEvent
            {
                Type = TaskEventType.Created,
                Task = created,
                UserId = userId,
                Username = GetUsername()
            });

            return created;
        }

        public async Task<bool> UpdateAsync(int id, int userId, TaskItem task)
        {
            var existing = await _repository.GetByIdAsync(id, userId);
            if (existing == null) return false;

            var wasCompleted = existing.IsCompleted;

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.IsCompleted = task.IsCompleted;
            existing.DueDate = task.DueDate.HasValue
                ? DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc)
                : null;

            await _repository.UpdateAsync(existing);

            if (!wasCompleted && task.IsCompleted)
            {
                await PublishEvent(new TaskEvent
                {
                    Type = TaskEventType.Completed,
                    Task = existing,
                    UserId = userId,
                    Username = GetUsername()
                });
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var task = await _repository.GetByIdAsync(id, userId);
            if (task == null) return false;

            await _repository.DeleteAsync(task);

            await PublishEvent(new TaskEvent
            {
                Type = TaskEventType.Deleted,
                Task = task,
                UserId = userId,
                Username = GetUsername()
            });

            return true;
        }
    }
}