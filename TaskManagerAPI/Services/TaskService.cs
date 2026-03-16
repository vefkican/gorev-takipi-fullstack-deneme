using TaskManagerAPI.Models.DTOs;
using TaskManagerAPI.Models.Entities;
using TaskManagerAPI.Repositories;

namespace TaskManagerAPI.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;

        public TaskService(ITaskRepository repository)
        {
            _repository = repository;
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

            return await _repository.CreateAsync(task);
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