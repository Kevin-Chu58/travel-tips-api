using System.Threading.Channels;
using static TravelTipsAPI.Services.BackgroundServices.ServiceInterface;

namespace TravelTipsAPI.Services.BackgroundServices
{
    public class StripeWebhookBackgroundTaskQueue : IStripeWebhookBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue;

        public StripeWebhookBackgroundTaskQueue(int capacity = 100)
        {
            // Bounded channel to prevent memory overflow
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
        }

        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            if (!_queue.Writer.TryWrite(workItem))
                throw new Exception("Queue is full");
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken
        )
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
