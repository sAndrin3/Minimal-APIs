using Microsoft.Extensions.Options;
using MinimalApi.ConnectionString;

namespace MinimalApi.Jobs;

public class RecurringTask : BackgroundService
{
    private Notifications _notifications;
    
    public RecurringTask(IOptionsMonitor<Notifications> optionsMonitor)
    {
        _notifications =  optionsMonitor.CurrentValue;

        optionsMonitor.OnChange(newNotifications =>
        {
            if (newNotifications.NotificationType != _notifications.NotificationType)
            {
                Console.WriteLine($"THE NOTIFICATION TYPE HAVE BEEN UPDATED TO {_notifications.NotificationType}");
            }
            
            _notifications = newNotifications;
        });
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            // Execute task...
            Console.WriteLine($"The notification type is {_notifications.NotificationType}");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
