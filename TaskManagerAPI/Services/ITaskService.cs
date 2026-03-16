using TaskManagerAPI.Models.DTOs;
using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllAsync(int userId, string? search, bool? isCompleted);
        Task<TaskItem?> GetByIdAsync(int id, int userId);
        Task<TaskItem> CreateAsync(int userId, CreateTaskDto dto);
        Task<bool> UpdateAsync(int id, int userId, TaskItem task);
        Task<bool> DeleteAsync(int id, int userId);
    }
}