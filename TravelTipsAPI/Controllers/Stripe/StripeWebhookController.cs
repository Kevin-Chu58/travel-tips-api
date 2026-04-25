using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TravelTipsAPI.Controllers.TravelTips;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;

namespace TravelTipsAPI.Controllers.Stripe
{
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class StripeWebhookController(
        IConfiguration config,
        IStripeWebhooksService stripeWebhooksService
    ) : TravelTipsControllerBase
    {
        private readonly string _webhookSecret =
            config["Stripe:Webhook"]
            ?? throw new ArgumentException("Stripe:Webhook not configured");

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                // 1. Verify and Parse
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                // 2. Delegate to Service
                stripeWebhooksService.HandleEvent(stripeEvent);

                return Ok(); // 3. Acknowledge quickly
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
