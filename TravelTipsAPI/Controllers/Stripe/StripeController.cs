using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.ViewModels.Stripe;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;

namespace TravelTipsAPI.Controllers.Stripe
{
    [Route("api/[controller]")]
    public class StripeController(
        IUsersService usersService,
        ISubscriptionsService subscriptionsService,
        IStripeService stripeService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Create a checkout session for Stripe
        /// </summary>
        /// <param name="request">stripe sessuion request</param>
        /// <returns>checkout session url</returns>
        [HttpPost]
        [Route("create-session")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<string>> CreateCheckoutSession(
            [FromBody] StripeSessionRequest request
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var user = usersService.GetUserById(userId);

            var activeSubscription = subscriptionsService.GetActiveSubscriptionByUserId(userId);
            if (activeSubscription != null)
            {
                return BadRequest(Messages.SubscriptionAlreadyActive);
            }

            StripeEnum.PriceIdMap.TryGetValue(request.Subscription, out var priceId);
            if (priceId is null)
            {
                return BadRequest(Messages.SubscriptionTypeInvalid);
            }

            var options = new SessionCreateOptions
            {
                SuccessUrl = $"{Global.URL}/subscription",
                CancelUrl = $"{Global.URL}/subscription",
                // Use existing Stripe customer ID, or null if it does not exist
                Customer = user.StripeCustomerId,
                // Optional: pre-fill the customer's email in the Stripe checkout form
                CustomerEmail = user.StripeCustomerId == null ? user.Email : null,
                Mode = "subscription",
                LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
                ClientReferenceId = userId.ToString(),
                Metadata = new Dictionary<string, string>
                {
                    { "subscription", request.Subscription.ToString() },
                    { "renew_subscription", user.RenewSubscription.ToString() },
                },
                Expand = ["subscription"],
            };

            try
            {
                var client = new StripeClient(stripeService.GetApiKey());
                var service = new SessionService(client);
                Session session = await service.CreateAsync(options);
                return Ok(session.Url); // Redirect user to this URL
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
