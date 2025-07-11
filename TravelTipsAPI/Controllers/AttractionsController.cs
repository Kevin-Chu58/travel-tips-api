using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Attractions
    /// </summary>
    /// <param name="attractionsService">attractions service</param>
    /// <param name="linksService">links service</param>
    [Route("api/[controller]")]
    public class AttractionsController(
        IAttractionsService attractionsService,
        ILinksService linksService
    ) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("v2/{id}")]
        [AllowAnonymous]
        public ActionResult<Attraction2ViewModel> GetAttractionById(int id)
        {
            try
            {
                var attraction = attractionsService.FindAttractionById(id);
                return Ok((Attraction2ViewModel)attraction);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get the search result contains a list of attractions with filter params
        /// </summary>
        /// <param name="name">attraction name</param>
        /// <param name="osmId">attraction osm id</param>
        /// <param name="osmType">attraction osm type</param>
        /// <param name="timestamp">timestamp</param>
        /// <returns>a list of attractions that satisfy the condition</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<AttractionSearchViewModel> GetAllAttractionsByParams(
            [FromQuery] string? name,
            long? osmId,
            string? osmType,
            long timestamp
        )
        {
            var attractionViewModels = attractionsService.GetHighlightsByParams(
                name,
                osmId,
                osmType,
                null
            );

            var attractionSearch = new AttractionSearchViewModel
            {
                Timestamp = timestamp,
                Attractions = attractionViewModels,
            };

            return Ok(attractionSearch);
        }

        /// <summary>
        /// Get search result of your attractions with filter params
        /// </summary>
        /// <param name="name">attraction name</param>
        /// <param name="osmId">attraction osm id</param>
        /// <param name="timestamp">timestamp</param>
        /// <returns>a list of attractions that satisfy the condition</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<AttractionSearchViewModel> GetYourAttractionsByParams(
            [FromQuery] string? name,
            long? osmId,
            string? osmType,
            long timestamp
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var attractionViewModels = attractionsService.GetHighlightsByParams(
                name,
                osmId,
                osmType,
                userId
            );

            var attractionSearch = new AttractionSearchViewModel
            {
                Timestamp = timestamp,
                Attractions = attractionViewModels,
            };

            return Ok(attractionSearch);
        }

        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public ActionResult<
            IEnumerable<AttractionHighlightsViewModel>
        > GetAttractionHighlightsByUserId(int id)
        {
            var ahViewModels = attractionsService.GetAttractionHighlightsByUserId(id);
            return Ok(ahViewModels);
        }

        /// <summary>
        /// Create a new attraction
        /// </summary>
        /// <param name="newAttraction">new attraction details</param>
        /// <returns>the new attraction</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<AttractionViewModel>> PostNewAttractionAsync(
            [FromBody] AttractionPostViewModel newAttraction
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // verify the ownership of link
            var myLinkIds = linksService.GetMyLinkIds(userId);
            if (
                newAttraction.LinkId != null
                && myLinkIds.All(linkId => linkId != newAttraction.LinkId)
            )
                return Unauthorized(Messages.AccessDenied);

            // validate the inputs
            var invalidParams = attractionsService.ValidatePost(newAttraction);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            // validate osm id
            if (newAttraction.OsmId <= 0)
                return BadRequest(Messages.OsmIdRestricted);

            // validate osm type
            if (TypeEnums.OsmTypes.All.All(osmType => osmType != newAttraction.OsmType))
                return BadRequest(Messages.OsmTypeInvalid);

            var attractionViewModel = await attractionsService.PostNewHighlightAsync(
                userId,
                newAttraction
            );

            return Ok(attractionViewModel);
        }

        /// <summary>
        /// Update an existing attraction you own
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <param name="attractionPatch">attraction details to be updated</param>
        /// <returns>the attraction up to date</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.ATTRACTIONS)]
        public async Task<ActionResult<AttractionViewModel>> PatchAttractionAsync(
            int id,
            [FromBody] AttractionPatchViewModel attractionPatch
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // verify the ownership of link
            var myLinkIds = linksService.GetMyLinkIds(userId);
            if (
                attractionPatch.LinkId != null
                && myLinkIds.All(linkId => linkId != attractionPatch.LinkId)
            )
                return Unauthorized(Messages.AccessDenied);

            // validate the inputs
            var invalidParams = attractionsService.ValidatePatch(attractionPatch);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            // validate osm id
            if (attractionPatch.OsmId <= 0)
                return BadRequest(Messages.OsmIdRestricted);

            // validate osm type
            if (TypeEnums.OsmTypes.All.All(osmType => osmType != attractionPatch.OsmType))
                return BadRequest(Messages.OsmTypeInvalid);

            var highlight = attractionsService.FindHighlightById(id);

            var attractionViewModel = await attractionsService.PatchHighlightAsync(
                highlight,
                attractionPatch
            );

            return Ok(attractionViewModel);
        }

        /// <summary>
        /// Delete an existing attraction
        /// </summary>
        /// <returns>the attraction deleted</returns>
        [HttpDelete]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<int[]>> DeleteAttractionAsync([FromBody] int[] ids)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // verify is the owner of all trip ids
            var isOwnerList = attractionsService.IsOwnerList(userId, ids);
            if (!isOwnerList)
            {
                return BadRequest(Messages.HighlightUnauthorized);
            }

            var idsDeleted = await attractionsService.DeleteHighlightAsync(ids);
            return Ok(idsDeleted);
        }
    }
}
