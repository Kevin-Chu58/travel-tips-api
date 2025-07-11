using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Highlights
    /// </summary>
    /// <param name="highlightsService">highlights service</param>
    [Route("api/[controller]")]
    public class HighlightsController(IHighlightsService highlightsService)
        : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a list of highlights by attraction id, optional other params
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <param name="userId">user id</param>
        /// <returns>a list of highlights</returns>
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<Highlight>> GetHighlightsByAttractionId(
            int id,
            [FromQuery] int? userId
        )
        {
            var highlights = highlightsService.GetHighlightsByParams(id, userId);
            return Ok(highlights.Select(h => (HighlightViewModel)h).ToList());
        }
    }
}
