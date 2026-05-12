using Microsoft.AspNetCore.Mvc;
using Stripe;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.Stripe;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;

namespace TravelTipsAPI.Controllers.Stripe
{
    [Route("api/[controller]")]
    public class StripeController(
        IUsersService usersService,
        ISubscriptionsService subscriptionsService,
        IAdsService adsService,
        IAdTargetsService adTargetsService,
        ITargetRulesService targetRulesService,
        IStripeService stripeService
    ) : TravelTipsControllerBase
    {
        // create checkout sessions

        /// <summary>
        /// Create a checkout session for Stripe, for membership subscription
        /// </summary>
        /// <param name="request">stripe sessuion request</param>
        /// <returns>checkout session url</returns>
        [HttpPost]
        [Route("create-session")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<string>> CreateCheckoutSession(
            [FromBody] ViewModels.Stripe.StripeRequest request
        )
        {
            return BadRequest("Paid Service is currently off.");

            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var user = usersService.GetUserById(userId);

            var activeSubscription = subscriptionsService.GetActiveSubscriptionByUserId(userId);
            if (activeSubscription != null)
                return BadRequest(Messages.SubscriptionAlreadyActive);

            try
            {
                var url = await stripeService.CreateSession(user, request);
                return Ok(url);
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Create a checkout session for Stripe, for ad target weights when ad does not have Stripe sub id
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="request">stripe session request</param>
        /// <returns>checkout session url</returns>
        [HttpPost]
        [Route("create-session/{id}/ad-weight")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult<string>> CreateCheckoutSessionOnAdWeights(
            int id,
            [FromBody] StripeAdWeightRequest request
        )
        {
            return BadRequest("Paid Service is currently off.");

            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var user = usersService.GetUserById(userId);

            var ad = adsService.FindAdById(id);
            if (ad == null)
                return NotFound(Messages.AdNotFound);

            // check if the target type and value exists on this ad
            var _adTarget = adTargetsService.FindAdTargetByParams(
                id,
                request.TargetType,
                request.TargetValue
            );
            if (_adTarget != null)
                return BadRequest(Messages.AdTargetAlreadyExists);

            try
            {
                var url = await stripeService.CreateSessionOnAdWeightsWithoutSubId(
                    user,
                    ad,
                    request
                );
                return Ok(url);
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }

        // preview invoices

        [HttpPost]
        [Route("preview-invoice/{id}/ad-weight/billing-cycle")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<
            ActionResult<StripeBillingCyclePreviewInvoiceResponse>
        > PreviewBillingCycleInvoiceOnAdWeights(int id)
        {
            var ad = adsService.FindAdById(id);
            if (ad == null)
                return NotFound(Messages.AdNotFound);
            try
            {
                var response = await stripeService.PreviewBillingCycleInvoiceOnAdWeights(ad);
                return Ok(response);
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Preview the upcoming invoice with more weights on ad the ad target is attached to
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="request">stripe ad weight request</param>
        /// <returns>a response on stripe invoice preview</returns>
        [HttpPost]
        [Route("preview-invoice/{id}/ad-weight")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<
            ActionResult<StripePreviewInvoiceResponse>
        > PreviewUpcomingInvoiceOnAdWeights(
            int id,
            [FromQuery] int? adTargetId,
            [FromBody] StripeAdWeightRequest request
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                var user = usersService.GetUserById(userId);

                var ad = adsService.FindAdById(id);
                if (ad == null)
                    return NotFound(Messages.AdNotFound);

                AdTarget? adTarget = null;
                if (adTargetId != null)
                {
                    adTarget = adTargetsService.FindAdTargetById((int)adTargetId);
                    if (adTarget == null)
                        return NotFound(Messages.AdTargetNotFound);
                }

                var response = await stripeService.PreviewUpcomingInvoiceOnAdWeights(
                    user,
                    ad,
                    request,
                    adTarget
                );
                return Ok(response);
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }

        // update subscriptions

        /// <summary>
        /// update the stripe subscription based on the new weight of the ad target
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="adTargetId">ad target id</param>
        /// <param name="request">stripe ad weight request</param>
        /// <returns></returns>
        [HttpPost]
        [Route("update-subscription/{id}/ad-weight")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult> UpdateSubscriptionOnAdWeights(
            int id,
            [FromQuery] int? adTargetId,
            [FromBody] StripeAdWeightRequest request
        )
        {
            return BadRequest("Paid Service is currently off.");

            if (request.Weight <= 0)
                return BadRequest(Messages.AdTargetWeightZeroInvalid);

            var ad = adsService.FindAdById(id);
            if (ad == null)
                return NotFound(Messages.AdNotFound);

            if (ad.StripeSubscriptionId == null)
                return BadRequest(Messages.AdStripeSubIdMissing);

            if (ad.SubStatus == "canceled")
                return BadRequest(Messages.AdSubscriptionCanceled);

            // check if the target type and value exists on this ad only when adTargetId is null
            // because ad target id is only null when creating a new ad target
            var _adTarget = adTargetsService.FindAdTargetByParams(
                id,
                request.TargetType,
                request.TargetValue
            );
            if (_adTarget != null && adTargetId == null)
                return BadRequest(Messages.AdTargetAlreadyExists);

            // try to find the ad target if provides the id, and return error if not found
            AdTarget? adTarget = null;
            if (adTargetId != null)
            {
                adTarget = adTargetsService.FindAdTargetById((int)adTargetId);
                if (adTarget == null)
                    return NotFound(Messages.AdTargetNotFound);
            }

            if (ad.StripeSubscriptionId == null)
                throw new Exception(Messages.AdStripeSubIdMissing);

            // check target rule whether the new weight meets the min weight requirement
            var targetRule = targetRulesService.GetTargetRule(
                request.TargetType,
                request.TargetValue
            );
            if (targetRule is null)
                throw new Exception(Messages.TargetRuleNotFound);

            if (request.Weight < targetRule.MinWeight)
                throw new Exception(Messages.TargetRuleMinWeightNotMet);

            // if the weight does not change,
            // only update the ad target in database without calling Stripe API to update subscription
            if (request.Weight == adTarget?.Weight)
            {
                await adTargetsService.UpdateAdTarget(adTarget, request);
                return Ok();
            }

            try
            {
                await stripeService.UpdateSubscriptionOnAdWeights(ad, request, adTarget);
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Cancel an ad target for the future billing cycles, and update the stripe subscription accordingly
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="adTargetId">ad target id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("update-subscription/{id}/cancel-ad-target")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult> UpdateSubscriptionOnDeleteAdTarget(
            int id,
            [FromQuery] int adTargetId
        )
        {
            return BadRequest("Paid Service is currently off.");

            var ad = adsService.FindAdById(id);
            if (ad == null)
                return NotFound(Messages.AdNotFound);

            var adTarget = adTargetsService.FindAdTargetById(adTargetId);
            if (adTarget == null)
                return NotFound(Messages.AdTargetNotFound);

            if (adTarget.IsPrimary)
                return BadRequest(Messages.AdTargetPrimaryTargetCannotBeCanceled);

            try
            {
                await stripeService.UpdateSubscriptionOnDeleteAdTarget(ad, adTarget);
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
