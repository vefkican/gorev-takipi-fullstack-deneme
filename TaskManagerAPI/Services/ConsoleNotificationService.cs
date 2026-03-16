namespace TaskManagerAPI.Services
{
    public class ConsoleNotificationService : INotificationService
    {
        private readonly ILogger<ConsoleNotificationService> _logger;

        public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendTaskCompletedAsync(string username, string taskTitle)
        {
            _logger.LogInformation(
                "🎉 Bildirim: {Username} kullanıcısı '{TaskTitle}' görevini tamamladı!",
                username, taskTitle);
            return Task.CompletedTask;
        }

        public Task SendTaskCreatedAsync(string username, string taskTitle)
        {
            _logger.LogInformation(
                "📝 Bildirim: {Username} kullanıcısı '{TaskTitle}' görevi oluşturdu!",
                username, taskTitle);
            return Task.CompletedTask;
        }
    }
}