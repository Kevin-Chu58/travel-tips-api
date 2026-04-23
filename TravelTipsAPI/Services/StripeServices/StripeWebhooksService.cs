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
        IServiceScopeFactory scopeFactory,
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
                            using var scope = scopeFactory.CreateScope();
                            var fulfillment =
                                scope.ServiceProvider.GetRequiredService<IStripeWebhooksFulfillService>();

                            // Inside the detailed fulfill methods, we mark as processed only IF the work succeeds
                            await fulfillment.FulfillSessionCompletedTaskAsync(
                                stripeEvent.Id,
                                session!
                            );
                        });
                    break;

                case EventTypes.InvoicePaid:
                    var invoice = stripeEvent.Data.Object as Invoice;

                    taskQueue.Enqueue(async token =>
                    {
                        using var scope = scopeFactory.CreateScope();
                        var fulfillment =
                            scope.ServiceProvider.GetRequiredService<IStripeWebhooksFulfillService>();
                        await fulfillment.FulfillInvoicePaidTaskAsync(stripeEvent.Id, invoice!);
                    });
                    break;

                case EventTypes.CustomerSubscriptionUpdated:
                    var subscription = stripeEvent.Data.Object as Subscription;

                    taskQueue.Enqueue(async token =>
                    {
                        using var scope = scopeFactory.CreateScope();
                        var fulfillment =
                            scope.ServiceProvider.GetRequiredService<IStripeWebhooksFulfillService>();
                        await fulfillment.FulfullCustomerSubscriptionUpdatedAsync(
                            stripeEvent.Id,
                            subscription!
                        );
                    });
                    break;

                case EventTypes.CustomerSubscriptionDeleted:
                    var deletedSubscription = stripeEvent.Data.Object as Subscription;

                    taskQueue.Enqueue(async token =>
                    {
                        using var scope = scopeFactory.CreateScope();
                        var fulfillment =
                            scope.ServiceProvider.GetRequiredService<IStripeWebhooksFulfillService>();
                        await fulfillment.FulfillCustomerSubscriptionDeletedTaskAsync(
                            stripeEvent.Id,
                            deletedSubscription!
                        );
                    });
                    break;

                case EventTypes.CustomerDeleted:
                    var customer = stripeEvent.Data.Object as Customer;

                    taskQueue.Enqueue(async token =>
                    {
                        using var scope = scopeFactory.CreateScope();
                        var fulfillment =
                            scope.ServiceProvider.GetRequiredService<IStripeWebhooksFulfillService>();
                        await fulfillment.FulfillCustomerDeletedTaskAsync(
                            stripeEvent.Id,
                            customer!
                        );
                    });
                    break;
            }
        }
    }
}
