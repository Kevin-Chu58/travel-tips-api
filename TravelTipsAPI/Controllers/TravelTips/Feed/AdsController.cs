using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Constants.Enums.ImageEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class AdsController(
        TravelTipsContext context,
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
        public ActionResult<IEnumerable<AdViewModel>> GetMyAdsByBusinessId(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var ads = adsService.GetAdsByParams(userId, id);

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
        public ActionResult<IEnumerable<AdViewModel>> GetAdsByParams(
            [FromQuery] int? userId,
            int? businessId,
            int? status
        )
        {
            AdStatus? statusEnum = status != null ? (AdStatus)status : null;
            var ads = adsService.GetAdsByParams(userId, businessId, statusEnum);

            // get all image urls of the ads
            //var imageIds = ads.Where(ad => ad.ImageId != null)
            //    .Select(ad => (int)ad.ImageId!)
            //    .ToArray();
            //var images = await imagesService.GetImagesByIds(imageIds);

            //var imageDict = images.ToDictionary(img => img.Id, img => img.Url);

            //foreach (var ad in ads)
            //{
            //    if (imageDict.TryGetValue((int)ad.ImageId, out var imageUrl))
            //    {
            //        ad.Picture = imageUrl;
            //    }
            //}

            return Ok(ads);
        }

        /// <summary>
        /// Get ad by id
        /// </summary>
        /// <param name="id">ad id</param>
        /// <returns>an ad with the id</returns>
        [HttpGet]
        [Route("{id}")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult<AdViewModel>> GetAdById(int id)
        {
            try
            {
                var ad = adsService.GetAdById(id);

                var images = await imagesService.GetImagesByIds([(int)ad.ImageId]);
                if (!images.Any())
                    return NotFound(Messages.ImageNotFound);

                var image = images.First();
                ad.Picture = image.Url;

                return Ok(ad);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
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
            [FromForm] AdPostViewModel newAd
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                var tx = await context.Database.BeginTransactionAsync();

                var image = await imagesService.PostNewImageAsync(
                    newAd.ImageFile,
                    userId,
                    null,
                    ImageType.Ad
                );
                newAd.ImageId = image.Id;

                var ad = await adsService.PostNewAd(newAd, userId, id);

                await tx.CommitAsync();

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
            [FromForm] AdPatchViewModel adPatch
        )
        {
            try
            {
                var ad = adsService.FindAdById(id);
                if (ad == null)
                    return BadRequest(Messages.AdNotFound);

                var tx = await context.Database.BeginTransactionAsync();

                if (adPatch.ImageFile != null)
                {
                    var image = imagesService.FindImageById(ad.ImageId);
                    if (image == null)
                        return NotFound(Messages.ImageNotFound);

                    var imageViewModel = imagesService.OverwriteImageAsync(
                        image,
                        adPatch.ImageFile
                    );
                }

                var updatedAd = await adsService.UpdateAd(ad, adPatch);

                await tx.CommitAsync();

                var images = await imagesService.GetImagesByIds([(int)updatedAd.ImageId]);
                if (!images.Any())
                    return NotFound(Messages.ImageNotFound);

                var _image = images.First();
                updatedAd.Picture = _image.Url;

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
        /// <param name="reason">reason to update</param>
        /// <returns>the new status</returns>
        [HttpPatch]
        [Route("{id}/status")]
        [HasRole(Role = UserRoles.REVIEWER)]
        public async Task<ActionResult<string>> UpdateAdStatus(
            int id,
            [FromQuery] int status,
            string? reason = null
        )
        {
            if (reason != null && reason.Length > 50)
                return BadRequest(Messages.AdSubLogReasonInvalid);

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

                var newStatus = await adsService.UpdateAdStatus(ad, (AdStatus)status, reason);
                return Ok(newStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ad sub logs

        /// <summary>
        /// Get a list of ad sub logs by ad id
        /// </summary>
        /// <param name="id">ad id</param>
        /// <returns>a list of ad sub logs</returns>
        [HttpGet]
        [Route("{id}/logs")]
        [IsOwner(Resource = Resources.ADS)]
        public ActionResult<IEnumerable<AdSubLogViewModel>> GetAdSubLogs(int id)
        {
            var logs = adsService.GetAdSubLogsByAdId(id);
            return Ok(logs);
        }
    }
}
