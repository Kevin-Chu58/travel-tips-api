using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Controllers.TravelTips
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
        public ActionResult<IEnumerable<HighlightViewModel>> GetHighlightsByAttractionId(
            int id,
            [FromQuery] int? userId
        )
        {
            var highlights = highlightsService.GetHighlightsByParams(id, userId);
            var highlightViewModels = highlights
                .Select(h => highlightsService.GetHighlightViewModel(h))
                .ToList();
            return Ok(highlightViewModels);
        }

        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<HighlightViewModel>> PostHighlightAsync(
            [FromBody] HighlightPostViewModel newHighlight
        )
        {
            if (newHighlight.Description.Length == 0)
                return BadRequest(Messages.HighlightDescriptionEmpty);

            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var highlightViewModel = await highlightsService.PostNewHighlightAsync(
                newHighlight,
                userId
            );
            return Ok(highlightViewModel);
        }

        /// <summary>
        /// Update a highlight description
        /// </summary>
        /// <param name="id">highlight id</param>
        /// <param name="description">highlight description to be updated</param>
        /// <returns>update highlight</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.HIGHLIGHTS)]
        public async Task<ActionResult<HighlightViewModel>> PatchHighlightAsync(
            int id,
            [FromBody] string description
        )
        {
            try
            {
                var highlight = highlightsService.FindHighlightById(id);
                var highlightViewModel = await highlightsService.UpdateHighlightAsync(
                    highlight,
                    description
                );

                return Ok(highlightViewModel);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.HIGHLIGHTS)]
        public async Task<ActionResult<HighlightViewModel>> DeleteHighlightAsync(int id)
        {
            var highlight = highlightsService.FindHighlightById(id);
            var highlightViewModel = await highlightsService.DeleteHighlightAsync(highlight);

            return Ok(highlightViewModel);
        }
    }
}
