using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Services
{
    // Event tipleri
    public enum TaskEventType
    {
        Created,
        Completed,
        Deleted
    }

    // Event objesi
    public class TaskEvent
    {
        public TaskEventType Type { get; set; }
        public TaskItem Task { get; set; } = null!;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    // Observer interface
    public interface ITaskEventHandler
    {
        Task HandleAsync(TaskEvent taskEvent);
    }
}