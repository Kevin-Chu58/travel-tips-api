using Stripe;
using Stripe.Checkout;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.Stripe;

namespace TravelTipsAPI.Services.StripeServices
{
    public class StripeSchema
    {
        public interface IStripeService
        {
            RequestOptions? GetRequestOptions();
            string GetApiKey();

            // create sessions
            Task<string> CreateSession(User user, ViewModels.Stripe.StripeRequest request);
            Task<string> CreateSessionOnAdWeightsWithoutSubId(
                User user,
                Ad ad,
                StripeAdWeightRequest request
            );

            // preview invoices
            Task<StripePreviewInvoiceResponse> PreviewUpcomingInvoiceOnAdWeights(
                User user,
                Ad ad,
                StripeAdWeightRequest request
            );

            // update subscriptions
            Task UpdateSubscriptionOnAdWeights(
                Ad ad,
                StripeAdWeightRequest request,
                AdTarget? adTarget
            );
            Task UpdateSubscriptionOnDeleteAdTarget(Ad ad, AdTarget adTarget);
        }

        public interface IStripeWebhooksService
        {
            void HandleEvent(Event stripeEvent);
        }

        public interface IStripeWebhooksFulfillService
        {
            // fulfillment
            Task FulfillSessionCompletedTaskAsync(string eventId, Session session);
            Task FulfillInvoicePaidTaskAsync(string eventId, Invoice invoice);
            Task FulfullCustomerSubscriptionUpdatedAsync(
                string eventId,
                Stripe.Subscription subscription
            );
            Task FulfillCustomerSubscriptionDeletedTaskAsync(
                string eventId,
                Stripe.Subscription subscription
            );
            Task FulfillCustomerDeletedTaskAsync(string eventId, Customer customer);

            // detail fulfillment functions

            // - session completed
            Task MembershipSubscriptionSessionCompleted(
                string eventId,
                int userId,
                Session session,
                Stripe.Subscription subscription
            );
            Task AdWeightSessionCompleted(
                string eventId,
                int userId,
                Session session,
                Stripe.Subscription subscription
            );

            // - invoice paid -> subscription_cycle
            Task MembershipSubscriptionNewCycleInvoicePaid(
                string eventId,
                Invoice invoice,
                Stripe.Subscription subscription
            );
            Task AdSubscriptionNewCycleInvoicePaid(
                string eventId,
                Invoice invoice,
                Stripe.Subscription subscription
            );

            // - invoice paid -> subscription update
            Task AdWeightMoreInvoicePaid(
                string eventId,
                int adTargetId,
                Invoice invoice,
                Stripe.Subscription subscription
            );
            Task AdWeightNewInvoicePaid(
                string eventId,
                Invoice invoice,
                Stripe.Subscription subscription
            );

            // - subscription updated > payment failed
            Task MembershipSubscriptionPaymentFailed(
                string eventId,
                Stripe.Subscription subscription
            );
            Task AdPaymentFailed(string eventId, Stripe.Subscription subscription);

            // - subscription updated > manual updates
            Task AdWeightLessSubscriptionUpdated(
                string eventId,
                int adTargetId,
                Stripe.Subscription subscription
            );
            Task AdWeightDeletedSubscriptionUpdated(string eventId, int adTargetId);

            // - subscription deleted
            Task MembershipSubscriptionDeleted(string eventId, Stripe.Subscription subscription);
            Task AdSubscriptionDeleted(string eventId, Stripe.Subscription subscription);

            // - customer deleted
            Task CustomerDeleted(string eventId, Customer customer);
        }
    }
}
