namespace TaskManagerAPI.Services
{
    public interface INotificationService
    {
        Task SendTaskCompletedAsync(string username, string taskTitle);
        Task SendTaskCreatedAsync(string username, string taskTitle);
    }
}