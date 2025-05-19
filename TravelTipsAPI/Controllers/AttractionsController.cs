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
        /// <summary>
        /// Get the search result contains a list of attractions with filter params
        /// </summary>
        /// <param name="name">attraction name</param>
        /// <param name="osmId">attraction osm id</param>
        /// <param name="timestamp">timestamp</param>
        /// <returns>a list of attractions that satisfy the condition</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<AttractionSearchViewModel> GetAllAttractionsByParams(
            [FromQuery] string? name,
            long? osmId,
            long timestamp
        )
        {
            var attractionViewModels = attractionsService.GetHighlightsByParams(name, osmId, null);

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
            long timestamp
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var attractionViewModels = attractionsService.GetHighlightsByParams(
                name,
                osmId,
                userId
            );

            var attractionSearch = new AttractionSearchViewModel
            {
                Timestamp = timestamp,
                Attractions = attractionViewModels,
            };

            return Ok(attractionSearch);
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

            var highlight = attractionsService.FindHighlightById(id);

            // validate the inputs
            var invalidParams = attractionsService.ValidatePatch(attractionPatch);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            var attractionViewModel = await attractionsService.PatchHighlightAsync(
                highlight,
                attractionPatch
            );

            return Ok(attractionViewModel);
        }

        /// <summary>
        /// Delete an existing attraction
        /// </summary>
        /// <param name="id">the id of the attraction to be deleted</param>
        /// <returns>the attraction deleted</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.ATTRACTIONS)]
        public async Task<ActionResult<AttractionViewModel>> DeleteAttractionAsync(int id)
        {
            var highlight = attractionsService.FindHighlightById(id);

            var attractionViewModel = await attractionsService.DeleteHighlightAsync(highlight);
            return Ok(attractionViewModel);
        }
    }
}
