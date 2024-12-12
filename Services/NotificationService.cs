using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Helpers;


namespace TaskManagement.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var currentDate = DateTime.UtcNow;

                    // Görev oluşturma bildirimi
                    var recentTasks = dbContext.TaskItems
                        .Where(t => t.CreatedAt >= currentDate.AddHours(-1) && !t.IsFlagged)
                        .ToList();

                    foreach (var task in recentTasks)
                    {
                        var user = await dbContext.Users.FindAsync(task.UserId);
                        if (user == null) continue;

                        var notification = new Notification
                        {
                            UserId = task.UserId,
                            Message = $"New task created: {task.Title}",
                            NotificationDate = currentDate,
                            IsRead = false,
                            Type = Notification.NotificationType.Push
                        };

                        dbContext.Notifications.Add(notification);
                        await PushNotificationHelper.SendPushAsync(task.UserId, notification.Message, dbContext);

                        task.IsFlagged = true; // Görevi işaretle
                    }

                    // Son tarih yaklaşan görev bildirimi
                    var upcomingTasks = dbContext.TaskItems
                        .Where(t => t.DueDate <= currentDate.AddDays(1) && t.DueDate >= currentDate && !t.IsFlagged)
                        .ToList();

                    foreach (var task in upcomingTasks)
                    {
                        var user = await dbContext.Users.FindAsync(task.UserId);
                        if (user == null) continue;

                        var notification = new Notification
                        {
                            UserId = task.UserId,
                            Message = $"Your task '{task.Title}' is due soon!",
                            NotificationDate = currentDate,
                            IsRead = false,
                            Type = Notification.NotificationType.Push
                        };

                        dbContext.Notifications.Add(notification);
                        await PushNotificationHelper.SendPushAsync(task.UserId, notification.Message, dbContext);

                        task.IsFlagged = true;
                    }

                    await dbContext.SaveChangesAsync();
                }

                // Her saat kontrol et
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}