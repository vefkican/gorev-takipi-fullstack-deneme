using TaskManagerAPI.Models.DTOs;
using TaskManagerAPI.Models.Entities;
using TaskManagerAPI.Repositories;

namespace TaskManagerAPI.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly INotificationService _notification;

        public TaskService(ITaskRepository repository, INotificationService notification)
        {
            _repository = repository;
            _notification = notification;
        }

        public async Task<PagedResult<TaskItem>> GetAllAsync(int userId, string? search, bool? isCompleted, int page, int pageSize)
        {
            return await _repository.GetAllAsync(userId, search, isCompleted, page, pageSize);
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
            await _notification.SendTaskCreatedAsync(userId.ToString(), task.Title);
            return created;
        }

        public async Task<bool> UpdateAsync(int id, int userId, TaskItem task)
        {
            var existing = await _repository.GetByIdAsync(id, userId);
            if (existing == null) return false;

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.IsCompleted = task.IsCompleted;
            existing.DueDate = task.DueDate.HasValue
                ? DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc)
                : null;

            await _repository.UpdateAsync(existing);

            // Task tamamlandıysa bildirim gönder
            if (task.IsCompleted && !existing.IsCompleted == false)
            {
                await _notification.SendTaskCompletedAsync(userId.ToString(), existing.Title);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var task = await _repository.GetByIdAsync(id, userId);
            if (task == null) return false;

            await _repository.DeleteAsync(task);
            return true;
        }
    }
}