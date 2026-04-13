using Stripe;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_plan;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Services.TravelTipsServices.Plan
{
    public class SubscriptionsService(TravelTipsContext context, IStripeService stripeService)
        : ISubscriptionsService
    {
        /// <summary>
        /// get the last subscription by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>the last subscription</returns>
        public Models.TravelTipsModels.Subscription? FindLastSubscriptionByUserId(int userId)
        {
            var subscription = context
                .Subscriptions.Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Start)
                .FirstOrDefault();
            return subscription;
        }

        /// <summary>
        /// find the active subscription by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>the active subscription</returns>
        public Models.TravelTipsModels.Subscription? FindActiveSubscriptionByUserId(int userId)
        {
            var subscription = context
                .Subscriptions.Where(s => s.UserId == userId && s.Status == "active")
                .FirstOrDefault();
            return subscription;
        }

        public Models.TravelTipsModels.Subscription? FindSubscriptionByStripeSubId(
            string stripeSubId
        )
        {
            var subscription = context
                .Subscriptions.Where(s => s.StripeSubscriptionId == stripeSubId)
                .FirstOrDefault();
            return subscription;
        }

        /// <summary>
        /// Get the active subscription by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>subscription view model</returns>
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
                    StripeSubscriptionId = s.StripeSubscriptionId,
                    Status = s.Status,
                    CanceledAt = s.CanceledAt.HasValue
                        ? s.CanceledAt.Value.DateTime
                        : (DateTime?)null,
                })
                .FirstOrDefault();

            return subscription;
        }

        /// <summary>
        /// Get a list of subscriptions based on user id, with pagination support using cursor and limit
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="cursor">general cursor</param>
        /// <param name="limit">limit</param>
        /// <returns>a list of subscriptions</returns>
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
                query = query.OrderByDescending(s => s.Start).Take(limit.Value);
            }
            var subscriptions = query
                .Select(s => new SubscriptionViewModel
                {
                    Id = s.Id,
                    Plan = s.Plan.Description,
                    Start = s.Start.DateTime,
                    End = s.End.HasValue ? s.End.Value.DateTime : DateTime.MaxValue, // if end is null, treat it as active indefinitely
                    TotalAmount = s.TotalAmount,
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
            var subscription = new Models.TravelTipsModels.Subscription
            {
                UserId = newSubscription.UserId,
                PlanId = newSubscription.PlanId,
                Start = DateTime.SpecifyKind(newSubscription.Start, DateTimeKind.Utc),
                End = DateTime.SpecifyKind(newSubscription.End, DateTimeKind.Utc),
                TotalAmount = newSubscription.TotalAmount,
                Status = "active", // new subscriptions are active by default
                StripeSubscriptionId = newSubscription.StripeSubscriptionId,
            };

            context.Subscriptions.Add(subscription);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// update an existing subscription
        /// </summary>
        /// <param name="subscription">subscription</param>
        /// <param name="subscriptionPatch">subscription details to be updated</param>
        /// <returns></returns>
        public async Task UpdateSubscription(
            Models.TravelTipsModels.Subscription subscription,
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

        /// <summary>
        /// Expires the active subscription for a specified user.
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns></returns>
        public async Task ExpireActiveSubscriptionByUserId(int userId)
        {
            var activeSubscription = context
                .Subscriptions.Where(s => s.UserId == userId && s.Status == "active")
                .FirstOrDefault();

            if (activeSubscription == null)
                return; // no active subscription to expire

            activeSubscription.Status = "canceled";
            activeSubscription.End = DateTime.UtcNow;
            context.Subscriptions.Update(activeSubscription);
            await context.SaveChangesAsync();
        }

        // subscription status

        /// <summary>
        /// Update the subscription status (auto-renew or not) in Stripe
        /// </summary>
        /// <param name="subId">subscription id</param>
        /// <param name="cancelSub">cancel subscription status</param>
        /// <returns></returns>
        public async Task UpdateSubscriptionStatus(string subId, bool cancelSub)
        {
            var service = new SubscriptionService();
            var serviceOptions = stripeService.GetRequestOptions();
            var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = cancelSub };

            await service.UpdateAsync(subId, options, serviceOptions);
        }
    }
}
