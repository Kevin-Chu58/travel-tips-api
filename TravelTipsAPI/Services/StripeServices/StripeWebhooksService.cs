using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using static TravelTipsAPI.Services.BackgroundServices.ServiceInterface;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RecordsSchema;

namespace TravelTipsAPI.Services.StripeServices
{
    public class StripeWebhooksService(
        IStripeWebhookBackgroundTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        IProcessedStripeEventsService processedStripeEventsService,
        ILogger<StripeWebhooksService> logger
    ) : IStripeWebhooksService
    {
        public void HandleEvent(Event stripeEvent)
        {
            // Do this BEFORE the queue to avoid unnecessary background tasks
            if (processedStripeEventsService.DoesProcessedEventExist(stripeEvent.Id))
            {
                logger.LogInformation("Duplicate event {Id} skipped.", stripeEvent.Id);
                return;
            }

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                case EventTypes.CheckoutSessionAsyncPaymentSucceeded:
                    var session = stripeEvent.Data.Object as Session;

                    if (
                        stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentSucceeded
                        || session!.PaymentStatus == "paid"
                    )
                        taskQueue.Enqueue(async token =>
                        {
                            using var scope = serviceProvider.CreateScope();
                            var fulfillment =
                                scope.ServiceProvider.GetRequiredService<IStripeWebhooksFulfillService>();

                            // Inside the detailed fulfill methods, we mark as processed only IF the work succeeds
                            await fulfillment.FulfillSessionCompletedTaskAsync(
                                stripeEvent.Id,
                                session!
                            );
                        });
                    break;
            }
        }
    }
}
