using Capitec.Business.Services.Abstractions;
using Capitec.Business.Services.Options;

namespace Capitec.Api.Services;

public sealed class OverdueNotificationWorker(
    IServiceScopeFactory scopeFactory,
    NotificationScheduleOptions options,
    ILogger<OverdueNotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("Scheduled overdue-notification simulation is disabled.");
            return;
        }

        var interval = TimeSpan.FromMinutes(options.IntervalMinutes);
        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IOverdueNotificationService>();
                var created = await service.SimulateAsync(stoppingToken);
                logger.LogInformation("Scheduled simulation recorded {NotificationCount} overdue reminders.", created.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Scheduled overdue-notification simulation is stopping due to cancellation.");
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Scheduled overdue-notification simulation failed.");
            }
        }
    }
}