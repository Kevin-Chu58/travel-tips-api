using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_feed;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Utils.ObjectUtils;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class BannersController(IBannersService bannersService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get public banners that are currently active (current date is between from and to)
        /// </summary>
        /// <returns>a list of public banners</returns>
        [HttpGet]
        [Route("public")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BannerViewModel>>> GetPublicBanners()
        {
            var banners = await bannersService.GetPublicBannerViewModels();
            return Ok(banners);
        }

        /// <summary>
        /// Get a banner by id
        /// </summary>
        /// <param name="id">banner id</param>
        /// <returns>the banner with the id</returns>
        [HttpGet]
        [Route("{id}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<BannerViewModel>> GetBannerById(int id)
        {
            var banner = await bannersService.GetBannerViewModelById(id);
            if (banner == null)
                return NotFound(Messages.BannerNotFound);
            return Ok(banner);
        }

        /// <summary>
        /// Get a list of banners by cursor
        /// </summary>
        /// <param name="cursor">general cursor</param>
        /// <param name="limit">limit</param>
        /// <returns>the search results of the banners</returns>
        [HttpGet]
        [Route("all")]
        [HasRole(Role = UserRoles.ADMIN)]
        public ActionResult<SearchResults<BannerSimpleViewModel>> GetBanners(
            string? cursor = null,
            int? limit = null
        )
        {
            limit ??= Global.BANNER_DEFAULT_LIMIT;

            // decode cursor if provided
            GeneralCursor? bannerCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                bannerCursor = DecodeCursor<GeneralCursor>(cursor);
                if (bannerCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            var banners = bannersService.GetBanners(bannerCursor, limit);

            // encode cursor
            var bannerList = banners.ToList();
            string? newCursor = null;
            if (bannerList.Count == limit)
            {
                var lastBanner = bannerList.Last();
                newCursor = EncodeCursor(new GeneralCursor { Id = lastBanner.Id });
            }

            var result = new SearchResults<BannerSimpleViewModel>
            {
                Results = bannerList,
                Cursor = newCursor,
            };

            return Ok(result);
        }

        /// <summary>
        /// Create a new banner
        /// </summary>
        /// <param name="newBanner">the new banner</param>
        /// <returns>the new banner created</returns>
        [HttpPost]
        [Route("")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<BannerViewModel>> CreateBanner(
            [FromBody] BannerPostViewModel newBanner
        )
        {
            var banner = await bannersService.PostNewBanner(newBanner);
            return Ok(banner);
        }

        /// <summary>
        /// Update an existing banner
        /// </summary>
        /// <param name="id">banner id</param>
        /// <param name="updatedBanner">banner info to be updated</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult> UpdateBanner(
            int id,
            [FromBody] BannerPatchViewModel updatedBanner
        )
        {
            var banner = bannersService.FindBannerById(id);
            if (banner == null)
                return NotFound(Messages.BannerNotFound);

            await bannersService.UpdateBanner(banner, updatedBanner);

            return Ok();
        }

        /// <summary>
        /// Delete a banner by id
        /// </summary>
        /// <param name="id">banner id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult> DeleteBanner(int id)
        {
            var banner = bannersService.FindBannerById(id);
            if (banner == null)
                return NotFound(Messages.BannerNotFound);

            await bannersService.DeleteBanner(banner);
            return Ok();
        }

        // stylings

        [HttpGet]
        [Route("stylings")]
        [HasRole(Role = UserRoles.ADMIN)]
        public ActionResult<IEnumerable<BannerStylingSimpleViewModel>> GetBannerStylings()
        {
            var stylings = bannersService.GetAllBannerStylings();
            return Ok(stylings);
        }

        [HttpGet]
        [Route("stylings/{id}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public ActionResult<BannerStylingViewModel> GetBannerStylingById(int id)
        {
            var styling = bannersService.FindBannerStylingById(id);
            if (styling == null)
                return NotFound(Messages.BannerNotFound);

            return Ok((BannerStylingViewModel)styling);
        }

        [HttpPost]
        [Route("stylings/{name}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<BannerStylingViewModel>> CreateBannerStyling(
            string name,
            [FromBody] string styling
        )
        {
            var isValid = bannersService.ValidateStyling(styling);
            if (!isValid)
                return BadRequest(Messages.BannerStylingInvalid);

            var bannerStyling = await bannersService.PostNewStyling(name, styling);
            return Ok(bannerStyling);
        }

        [HttpPatch]
        [Route("stylings/{id}")]
        [HasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<BannerStylingViewModel>> UpdateBannerStyling(
            int id,
            [FromBody] BannerStylingPatchViewModel bannerStylingPatch
        )
        {
            var bannerStyling = bannersService.FindBannerStylingById(id);
            if (bannerStyling == null)
                return NotFound(Messages.BannerStylingNotFound);

            var isValid =
                bannerStylingPatch.Styling == null
                || bannersService.ValidateStyling(bannerStylingPatch.Styling);
            if (!isValid)
                return BadRequest(Messages.BannerStylingInvalid);

            var result = await bannersService.UpdateStyling(bannerStyling, bannerStylingPatch);
            return Ok(result);
        }
    }
}
