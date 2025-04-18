using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Attractions
    /// </summary>
    /// <param name="attractionsService">attractions service</param>
    [Route("api/[controller]")]
    public class AttractionsController(IAttractionsService attractionsService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get the search result contains a list of attractions with filter params in public trips
        /// </summary>
        /// <param name="name">attraction name</param>
        /// <param name="osmId">attraction osm id</param>
        /// <param name="time">timestamp</param>
        /// <returns>a list of attractions that satisfy the condition</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<AttractionSearchViewModel> GetPublicAttractionsByParams([FromQuery] string? name, int? osmId, int time)
        {
            var attractionViewModels = attractionsService.GetAttractionsByParams(
                name,
                osmId,
                true,
                null
            );

            var attractionSearch = new AttractionSearchViewModel
            {
                TimeStamp = time,
                Attractions = attractionViewModels
            };

            return Ok(attractionSearch);
        }

        /// <summary>
        /// Get search result of your attractions with filter params
        /// </summary>
        /// <param name="name">attraction name</param>
        /// <param name="osmId">attraction osm id</param>
        /// <param name="time">timestamp</param>
        /// <returns>a list of attractions that satisfy the condition</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<AttractionSearchViewModel> GetYourAttractionsByParams([FromQuery] string? name, int? osmId, int time)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var attractionViewModels = attractionsService.GetAttractionsByParams(
                name,
                osmId,
                null,
                userId
            );

            var attractionSearch = new AttractionSearchViewModel
            {
                TimeStamp = time,
                Attractions = attractionViewModels
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
        public async Task<ActionResult<AttractionViewModel>> PostNewAttractionAsync([FromBody] AttractionPostViewModel newAttraction)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var attractionViewModel = await attractionsService.PostNewAttractionAsync(userId, newAttraction);

            return Ok(attractionViewModel);
        }

        /// <summary>
        /// Update an existing attraction you own
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <param name="attraction">attraction details to be updated</param>
        /// <returns>the attraction up to date</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.ATTRACTIONS)]
        public async Task<ActionResult<AttractionViewModel>> PatchAttractionAsync(int id, [FromBody] AttractionPatchViewModel attraction)
        {
            var attractionViewModel = await attractionsService.PatchAttractionAsync(id, attraction);

            return Ok(attractionViewModel);
        }
    }
}
