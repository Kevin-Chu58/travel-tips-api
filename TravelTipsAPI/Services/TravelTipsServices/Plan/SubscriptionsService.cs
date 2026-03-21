using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_plan;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Services.TravelTipsServices.Plan
{
    public class SubscriptionsService(TravelTipsContext context) : ISubscriptionsService
    {
        public Subscription? FindLastSubscriptionByUserId(int userId)
        {
            var subscription = context
                .Subscriptions.Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Start)
                .FirstOrDefault();
            return subscription;
        }

        public SubscriptionViewModel? GetActiveSubscriptionByUserId(int userId)
        {
            var subscription = context
                .Subscriptions.Where(s => s.UserId == userId && s.Status == "active")
                .Select(s => new SubscriptionViewModel
                {
                    Id = s.Id,
                    Plan = s.Plan.Description,
                    Start = s.Start.DateTime,
                    End = s.End.HasValue ? s.End.Value.DateTime : DateTime.MaxValue, // if end is null, treat it as active indefinitely
                    TotalAmount = s.TotalAmount,
                    Currency = s.Currency,
                    StripeSubscriptionId = s.StripeSubscriptionId,
                    Status = s.Status,
                    CanceledAt = s.CanceledAt.HasValue
                        ? s.CanceledAt.Value.DateTime
                        : (DateTime?)null,
                })
                .FirstOrDefault();

            return subscription;
        }

        public IEnumerable<SubscriptionViewModel> GetSubscriptionsByUserIdWithCursor(
            int userId,
            GeneralCursor? cursor = null,
            int? limit = null
        )
        {
            var query = context.Subscriptions.AsQueryable().Where(s => s.UserId == userId);

            if (cursor != null)
            {
                query = query.Where(s => s.Id < cursor.Id);
            }

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }
            var subscriptions = query
                .Select(s => new SubscriptionViewModel
                {
                    Id = s.Id,
                    Plan = s.Plan.Description,
                    Start = s.Start.DateTime,
                    End = s.End.HasValue ? s.End.Value.DateTime : DateTime.MaxValue, // if end is null, treat it as active indefinitely
                    TotalAmount = s.TotalAmount,
                    Currency = s.Currency,
                    StripeSubscriptionId = s.StripeSubscriptionId,
                    Status = s.Status,
                    CanceledAt = s.CanceledAt.HasValue
                        ? s.CanceledAt.Value.DateTime
                        : (DateTime?)null,
                })
                .ToList();

            return subscriptions;
        }

        // no return, just add the subscription to the database
        // triggered by Stripe webhook when a checkout session is completed
        public async Task AddSubscription(SubscriptionPostViewModel newSubscription)
        {
            var subscription = new Subscription
            {
                UserId = newSubscription.UserId,
                PlanId = newSubscription.PlanId,
                Start = DateTime.SpecifyKind(newSubscription.Start, DateTimeKind.Utc),
                End = DateTime.SpecifyKind(newSubscription.End, DateTimeKind.Utc),
                TotalAmount = newSubscription.TotalAmount,
                Currency = newSubscription.Currency,
                Status = "active", // new subscriptions are active by default
                StripeSubscriptionId = newSubscription.StripeSubscriptionId,
            };

            context.Subscriptions.Add(subscription);
            await context.SaveChangesAsync();
        }

        public async Task UpdateSubscription(
            Subscription subscription,
            SubscriptionPatchViewModel subscriptionPatch
        )
        {
            if (subscriptionPatch.Status != null)
            {
                subscription.Status = subscriptionPatch.Status;
                if (subscriptionPatch.Status == "canceled")
                {
                    subscription.CanceledAt = DateTime.UtcNow;
                }
            }

            context.Subscriptions.Update(subscription);
            await context.SaveChangesAsync();
        }
    }
}
