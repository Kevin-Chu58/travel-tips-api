using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class AdTargetsController(IAdTargetsService adTargetsService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a list of ad targets by ad id
        /// </summary>
        /// <param name="id">ad id</param>
        /// <returns>a list of ad targets</returns>
        [HttpGet]
        [Route("{id}")]
        [IsOwner(Resource = Resources.ADS)]
        public ActionResult<IEnumerable<AdTargetViewModel>> GetAdTargetsByAdId(int id)
        {
            var result = adTargetsService.GetAdTargetsByAdId(id);
            return Ok(result);
        }

        /// <summary>
        /// Decrease the weight of an ad target
        /// </summary>
        /// <param name="id">ad id, for authorization check only</param>
        /// <param name="targetId">tar</param>
        /// <param name="decrement"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/ad-target/{targetId}")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult> DecreaseAdTargetWeight(
            int id,
            int targetId,
            [FromBody] int decrement
        )
        {
            try
            {
                var adTarget = adTargetsService.FindAdTargetById(targetId);
                if (adTarget == null)
                    return NotFound(Messages.AdTargetNotFound);

                if (adTarget.AdId != id)
                    return BadRequest(Messages.AdTargetNotBelongToAd);

                // TODO - invoke Stripe API to update the quantity in
                // the Stripe item id associated with the ad target

                await adTargetsService.DecreaseAdTargetWeight(adTarget, decrement);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancel/reinstate an ad target
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="targetId">ad target id</param>
        /// <param name="cancel">cancel status</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/ad-target/{targetId}/cancel")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult> CancelAdTarget(
            int id,
            int targetId,
            [FromQuery] bool cancel
        )
        {
            try
            {
                var adTarget = adTargetsService.FindAdTargetById(targetId);
                if (adTarget == null)
                    return NotFound(Messages.AdTargetNotFound);

                if (adTarget.AdId != id)
                    return BadRequest(Messages.AdTargetNotBelongToAd);

                // TODO - invoke Stripe API to cancel the Stripe item id associated with the ad target

                await adTargetsService.CancelAdTarget(adTarget);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
