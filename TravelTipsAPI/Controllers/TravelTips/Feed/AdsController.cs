using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class AdsController(
        IBusinessesService businessesService,
        IAdsService adsService,
        IImagesService imagesService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get my ads by business id
        /// </summary>
        /// <param name="id">business id</param>
        /// <returns>a list of my ads in that business</returns>
        [HttpGet]
        [Route("my/{id}")]
        [IsOwner(Resource = Resources.BUSINESSES)]
        public async Task<ActionResult<IEnumerable<AdViewModel>>> GetMyAdByBusinessId(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var ads = adsService.GetAdsByParams(userId, id);

            // get all image urls of the ads
            var imageIds = ads.Select(ad => ad.ImageId).ToArray();
            var images = await imagesService.GetImagesByIds(imageIds);

            var imageDict = images.ToDictionary(img => img.Id, img => img.Url);

            foreach (var ad in ads)
            {
                if (imageDict.TryGetValue(ad.ImageId, out var imageUrl))
                {
                    ad.Picture = imageUrl;
                }
            }

            return Ok(ads);
        }

        /// <summary>
        /// Get a list of ads by params (user id, business id, ad status)
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="businessId">business id</param>
        /// <param name="status">ad status</param>
        /// <returns>a list of ads with that params</returns>
        [HttpGet]
        [Route("")]
        [HasRole(Role = UserRoles.REVIEWER)]
        public async Task<ActionResult<IEnumerable<AdViewModel>>> GetAdsByParams(
            [FromQuery] int? userId,
            [FromQuery] int? businessId,
            [FromQuery] AdStatus? status
        )
        {
            var ads = adsService.GetAdsByParams(userId, businessId, status);

            // get all image urls of the ads
            var imageIds = ads.Select(ad => ad.ImageId).ToArray();
            var images = await imagesService.GetImagesByIds(imageIds);

            var imageDict = images.ToDictionary(img => img.Id, img => img.Url);

            foreach (var ad in ads)
            {
                if (imageDict.TryGetValue(ad.ImageId, out var imageUrl))
                {
                    ad.Picture = imageUrl;
                }
            }

            return Ok(ads);
        }

        /// <summary>
        /// Create new ad in pending status under a business
        /// </summary>
        /// <param name="id">business id</param>
        /// <param name="newAd">new ad</param>
        /// <returns>the new ad</returns>
        [HttpPost]
        [Route("{id}")]
        [IsOwner(Resource = Resources.BUSINESSES)]
        public async Task<ActionResult<AdViewModel>> PostNewAd(
            int id,
            [FromBody] AdPostViewModel newAd
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                var ad = await adsService.PostNewAd(newAd, userId, id);

                return Ok(ad);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an ad details
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="adPatch">ad details to be updated</param>
        /// <returns>the updated ad</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult<AdViewModel>> UpdateAd(
            int id,
            [FromBody] AdPatchViewModel adPatch
        )
        {
            try
            {
                var ad = adsService.FindAdById(id);
                if (ad == null)
                    return BadRequest(Messages.AdNotFound);

                var updatedAd = await adsService.UpdateAd(ad, adPatch);
                return Ok(updatedAd);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an ad active status (active/inactive)
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="isActive">ad active status</param>
        /// <returns>the new status</returns>
        [HttpPatch]
        [Route("{id}/active-status")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult<string>> UpdateAdActiveStatus(
            int id,
            [FromQuery] bool isActive
        )
        {
            try
            {
                var ad = adsService.FindAdById(id);
                if (ad == null)
                    return BadRequest(Messages.AdNotFound);

                // Get the business of the ad to check if the business is active
                var business = businessesService.FindBusinessById(ad.BusinessId);
                if (business == null)
                    return BadRequest(Messages.BusinessNotFound);

                // Only when the business is active, the ad can be set to active
                if (business.Status != GetAdStatusStr(AdStatus.Active))
                {
                    return BadRequest(Messages.BusinessIsNotActive);
                }

                var newStatus = await adsService.UpdateAdActiveStatus(ad, isActive);
                return Ok(newStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an ad status (pending/active/inactive/request change/denied)
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="status">new ad status</param>
        /// <returns>the new status</returns>
        [HttpPatch]
        [Route("{id}/status")]
        [HasRole(Role = UserRoles.REVIEWER)]
        public async Task<ActionResult<string>> UpdateAdStatus(int id, [FromQuery] AdStatus status)
        {
            try
            {
                var ad = adsService.FindAdById(id);
                if (ad == null)
                    return BadRequest(Messages.AdNotFound);

                // Get the business of the ad to check if the business is active
                var business = businessesService.FindBusinessById(ad.BusinessId);
                if (business == null)
                    return BadRequest(Messages.BusinessNotFound);

                // Only when the business is active, the ad can be set to active
                if (business.Status != GetAdStatusStr(AdStatus.Active))
                {
                    return BadRequest(Messages.BusinessIsNotActive);
                }

                var newStatus = await adsService.UpdateAdStatus(ad, status);
                return Ok(newStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
