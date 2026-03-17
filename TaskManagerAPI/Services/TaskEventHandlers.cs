using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Services
{
    // Log handler
    public class LoggingTaskEventHandler : ITaskEventHandler
    {
        private readonly ILogger<LoggingTaskEventHandler> _logger;

        public LoggingTaskEventHandler(ILogger<LoggingTaskEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(TaskEvent taskEvent)
        {
            switch (taskEvent.Type)
            {
                case TaskEventType.Created:
                    _logger.LogInformation("📝 Task oluşturuldu: {Title} (Kullanıcı: {Username})",
                        taskEvent.Task.Title, taskEvent.Username);
                    break;
                case TaskEventType.Completed:
                    _logger.LogInformation("✅ Task tamamlandı: {Title} (Kullanıcı: {Username})",
                        taskEvent.Task.Title, taskEvent.Username);
                    break;
                case TaskEventType.Deleted:
                    _logger.LogInformation("🗑️ Task silindi: {Title} (Kullanıcı: {Username})",
                        taskEvent.Task.Title, taskEvent.Username);
                    break;
            }
            return Task.CompletedTask;
        }
    }

    // Notification handler
    public class NotificationTaskEventHandler : ITaskEventHandler
    {
        private readonly INotificationService _notification;

        public NotificationTaskEventHandler(INotificationService notification)
        {
            _notification = notification;
        }

        public async Task HandleAsync(TaskEvent taskEvent)
        {
            switch (taskEvent.Type)
            {
                case TaskEventType.Created:
                    await _notification.SendTaskCreatedAsync(
                        taskEvent.Username, taskEvent.Task.Title);
                    break;
                case TaskEventType.Completed:
                    await _notification.SendTaskCompletedAsync(
                        taskEvent.Username, taskEvent.Task.Title);
                    break;
            }
        }
    }
}