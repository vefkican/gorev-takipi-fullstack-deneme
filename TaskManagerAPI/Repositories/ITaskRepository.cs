using TaskManagerAPI.Models.DTOs;
using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Repositories
{
    public interface ITaskRepository
    {
        Task<PagedResult<TaskItem>> GetAllAsync(int userId, string? search, bool? isCompleted, int page, int pageSize, string? sortBy = null); Task<TaskItem?> GetByIdAsync(int id, int userId);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
    }
}