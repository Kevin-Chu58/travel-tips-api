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
        /// Get the analytics of an ad target
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="targetId">ad target id</param>
        /// <returns>the analytics</returns>
        [HttpGet]
        [Route("{id}/ad-target/{targetId}/analytics")]
        public ActionResult<AdTargetAnalytics> GetAdTargetAnalytics(int id, int targetId)
        {
            var adTarget = adTargetsService.FindAdTargetById(targetId);

            if (adTarget == null)
                return NotFound(Messages.AdTargetNotFound);

            if (adTarget.AdId != id)
                return BadRequest(Messages.AdTargetNotBelongToAd);

            var analytics = adTargetsService.GetAdTargetRanking(adTarget);
            return Ok(analytics);
        }

        /// <summary>
        /// Set an ad target as primary for the ad
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="targetId">ad target id</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/ad-target/{targetId}/primary")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult> SetAdTargetAsPrimary(int id, int targetId)
        {
            try
            {
                var adTarget = adTargetsService.FindAdTargetById(targetId);

                if (adTarget == null)
                    return NotFound(Messages.AdTargetNotFound);

                if (adTarget.AdId != id)
                    return BadRequest(Messages.AdTargetNotBelongToAd);

                await adTargetsService.SetAdTargetAsPrimary(adTarget);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
