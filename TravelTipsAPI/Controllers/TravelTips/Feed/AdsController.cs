using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Constants.Enums.AdTargetEnum;
using static TravelTipsAPI.Constants.Enums.ImageEnum;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;
using static TravelTipsAPI.Utils.ObjectUtils;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class AdsController(
        TravelTipsContext context,
        //IUsersService usersService,
        IRegionsService regionsService,
        IBusinessesService businessesService,
        IAdsService adsService,
        IImagesService imagesService,
        IStripeService stripeService
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

            // get all image urls of the ads
            //var imageIds = ads.Select(ad => ad.ImageId).ToArray();
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

        [HttpGet]
        [Route("pending")]
        [HasRole(Role = UserRoles.REVIEWER)]
        public ActionResult<IEnumerable<AdViewModel>> GetPendingAds()
        {
            var ads = adsService.GetAdsByParams(
                null,
                null,
                AdStatus.Pending,
                Global.AD_DEFAULT_LIMIT
            );

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

        [HttpGet]
        [Route("feed")]
        [AllowAnonymous]
        public async Task<ActionResult<AdViewModel?>> GetAdFeed(
            [FromQuery] string? title,
            int? createdBy = null,
            string? countrySlug = null,
            string? stateSlug = null,
            int? budget = null
        )
        {
            List<(string, string)> targets = [];

            if (title != null && title.Length > 2)
                targets.Add((GetAdTargetStr(AdTargetEnum.AdTarget.Keyword)!, title));

            if (createdBy != null)
                targets.Add(
                    (GetAdTargetStr(AdTargetEnum.AdTarget.CreatedBy)!, createdBy.ToString()!)
                );

            if (countrySlug != null)
                targets.Add(
                    (
                        GetAdTargetStr(AdTargetEnum.AdTarget.Region)!,
                        regionsService
                            .GetRegionByCountryAndState(countrySlug, stateSlug)
                            ?.Id.ToString() ?? ""
                    )
                );

            if (budget != null)
                targets.Add((GetAdTargetStr(AdTargetEnum.AdTarget.Budget)!, budget.ToString()!));

            var ad = adsService.GetAdFeed(targets);

            if (ad == null)
                return Ok();

            var adViewModel = adsService.GetAdById(ad.Id);

            var images = await imagesService.GetImagesByIds([(int)ad.ImageId]);
            if (!images.Any())
                return NotFound(Messages.ImageNotFound);

            var image = images.First();
            adViewModel.Picture = image.Url;

            return Ok(adViewModel);
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

                    var imageViewModel = await imagesService.OverwriteImageAsync(
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
        /// <param name="cursor">pagination cursor</param>
        /// <param name="limit">pagination limit</param>
        /// <returns>a list of ad sub logs</returns>
        [HttpGet]
        [Route("{id}/logs")]
        [IsOwner(Resource = Resources.ADS)]
        public ActionResult<SearchResults<AdSubLogViewModel>> GetAdSubLogs(
            int id,
            [FromQuery] string? cursor = null,
            int? limit = null
        )
        {
            limit ??= Global.AD_SUB_LOG_DEFAULT_LIMIT;

            // decode cursor if provided
            GeneralCursor? adSubLogCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                adSubLogCursor = DecodeCursor<GeneralCursor>(cursor);
                if (adSubLogCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            var logs = adsService.GetAdSubLogsByAdIdWithCursor(id, adSubLogCursor, limit);

            // encode cursor
            var logHistory = logs.ToList();
            string? newCursor = null;
            if (logHistory.Count == limit)
            {
                var lastAdSubLog = logHistory.Last();
                newCursor = EncodeCursor(new GeneralCursor { Id = lastAdSubLog.Id });
            }

            var result = new SearchResults<AdSubLogViewModel>
            {
                Results = logHistory,
                Cursor = newCursor,
            };

            return Ok(result);
        }

        // subscription

        /// <summary>
        /// Update an ad subscription renew status in Stripe (auto-renew or not)
        /// </summary>
        /// <param name="id">ad id</param>
        /// <param name="renewSubscription">new renew subscription status</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/renewSubscription")]
        [IsOwner(Resource = Resources.ADS)]
        public async Task<ActionResult> UpdateRenewSubscription(
            int id,
            [FromQuery] bool renewSubscription
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var ad = adsService.FindAdById(id);
            if (ad == null)
                return BadRequest(Messages.AdNotFound);

            if (ad.StripeSubscriptionId == null)
                return BadRequest(Messages.AdStripeSubIdMissing);

            try
            {
                await stripeService.UpdateSubscriptionStatus(
                    ad.StripeSubscriptionId,
                    cancelSub: !renewSubscription
                );
                await adsService.UpdateAdSubscriptionRenewal(ad, renewSubscription);
                return Ok();
            }
            catch (Exception)
            {
                try
                {
                    await stripeService.UpdateSubscriptionStatus(
                        ad.StripeSubscriptionId,
                        cancelSub: renewSubscription
                    );
                    return Ok();
                }
                catch (Exception rollbackEx)
                {
                    // Log the rollback failure
                    Console.WriteLine(
                        $"Stripe renew subscription status rollback failed: {rollbackEx.Message}"
                    );
                    return BadRequest(rollbackEx.Message);
                }
            }
        }
    }
}
