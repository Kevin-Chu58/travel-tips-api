using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.Services.WikiCommonsServices;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.WikiCommonsServices.WikiCommonsSchema;

namespace TravelTipsAPI.Controllers.WikiCommons
{
    [Route("api/[controller]")]
    public class WikiCommonsController(
        IWikiCommonsService wikiCommonsService,
        IAttractionsService attractionsService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a list of wiki images by attraction id
        /// </summary>
        /// <param name="attractionId">attraction id</param>
        /// <returns>a list of wiki images of the attraction</returns>
        [HttpGet]
        [Route("{attractionId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<WikiImage>>> GetWikiImagesByAttractionId(
            int attractionId
        )
        {
            var attraction = attractionsService.FindAttractionById(attractionId);

            try
            {
                var searchString = $"{attraction.Title} {attraction.City}";
                var wikiImages = await wikiCommonsService.SearchImagesByTitleAsync(searchString);

                return Ok(wikiImages);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
