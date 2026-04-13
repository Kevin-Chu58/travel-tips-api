namespace TravelTipsAPI.Services.BackgroundServices
{
    public class ServiceInterface
    {
        public interface IStripeWebhookBackgroundTaskQueue
        {
            void Enqueue(Func<CancellationToken, Task> workItem);
            Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
        }
    }
}
