using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync(int userId, string? search, bool? isCompleted);
        Task<TaskItem?> GetByIdAsync(int id, int userId);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
    }
}