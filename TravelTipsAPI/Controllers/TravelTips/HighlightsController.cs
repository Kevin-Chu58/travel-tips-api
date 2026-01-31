using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Constants.OrderBy.HighlightOrderBy;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Utils.ObjectUtils;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Controllers.TravelTips
{
    /// <summary>
    /// The controller of Highlights
    /// </summary>
    /// <param name="highlightsService">highlights service</param>
    [Route("api/[controller]")]
    public class HighlightsController(
        IUsersService usersService,
        IHighlightsService highlightsService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get highlights by params
        /// </summary>
        /// <param name="attractionId">the attraction id</param>
        /// <param name="createdByAuthId">the creator user id</param>
        /// <param name="cursor">the pagination cursor</param>
        /// <param name="highlightOrderByEnum">the order by enum</param>
        /// <returns>search result of highlights that fits the params with cursor optionally</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<SearchResults<HighlightViewModel>> GetHighlightsByAttractionId(
            [FromQuery] int? attractionId,
            string? createdByAuthId,
            string? cursor,
            HighlightOrderByEnum? highlightOrderByEnum = null
        )
        {
            // get createdBy user id (not user's userId)
            int? createdBy = null;
            if (!string.IsNullOrEmpty(createdByAuthId))
            {
                var createdByUser = usersService.GetUserByUserId(createdByAuthId);
                if (createdByUser is null)
                    return NotFound(Messages.UserNotFound);
                createdBy = createdByUser.Id;
            }

            // set default order by id desc if no order provided
            if (highlightOrderByEnum is null)
                highlightOrderByEnum = HighlightOrderByEnum.Newest;

            // decode cursor if provided
            HighlightCursor? highlightCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                highlightCursor = DecodeCursor<HighlightCursor>(cursor);
                if (highlightCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            var highlightViewModels = highlightsService.GetHighlightsByParams(
                attractionId: attractionId,
                createdBy: createdBy,
                cursor: highlightCursor,
                highlightOrderByEnum: highlightOrderByEnum,
                limit: Global.HIGHLIGHT_DEFAULT_LIMIT
            );

            // encode cursor
            var highlightList = highlightViewModels.ToList();
            string? newCursor = null;
            if (highlightList.Count > 0)
            {
                var lastHighlight = highlightList.Last();
                newCursor = EncodeCursor(
                    new HighlightCursor
                    {
                        Id = lastHighlight.Id,
                        UsageCount = lastHighlight.UsageCount,
                    }
                );
            }

            var result = new SearchResults<HighlightViewModel>
            {
                Cursor = newCursor,
                Results = highlightViewModels,
            };

            return Ok(result);
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
        /// <param name="highlightPatch">highlight patch</param>
        /// <returns>update highlight</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.HIGHLIGHTS)]
        public async Task<ActionResult<HighlightViewModel>> PatchHighlightAsync(
            int id,
            [FromBody] HighlightPatchViewModel highlightPatch
        )
        {
            var highlight = highlightsService.FindHighlightById(id);

            if (highlight is null)
                return NotFound(Messages.HighlightNotFound);

            try
            {
                var highlightViewModel = await highlightsService.UpdateHighlightAsync(
                    highlight,
                    highlightPatch.Description
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
            if (highlight is null)
                return NotFound(Messages.HighlightNotFound);

            var highlightViewModel = await highlightsService.DeleteHighlightAsync(highlight);

            return Ok(highlightViewModel);
        }
    }
}
