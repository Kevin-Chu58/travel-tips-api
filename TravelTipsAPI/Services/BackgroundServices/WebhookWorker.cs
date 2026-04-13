using static TravelTipsAPI.Services.BackgroundServices.ServiceInterface;

namespace TravelTipsAPI.Services.BackgroundServices
{
    public class WebhookWorker(
        IStripeWebhookBackgroundTaskQueue taskQueue,
        ILogger<WebhookWorker> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Wait for a task to be enqueued
                var workItem = await taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    // Process the task
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Log errors (important for background jobs!)
                    logger.LogError(ex, "Error occurred executing webhook background task.");
                }
            }
        }
    }
}
