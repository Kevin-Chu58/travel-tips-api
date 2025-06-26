using System.Security.Claims;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.Services;
using TravelTipsAPI.Utils;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Links
    /// </summary>
    [Route("api/[controller]")]
    public class LinksController(ILinksService linksService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get the link search result by name from the links you own
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="timestamp">the time when the http request created</param>
        /// <returns>a link search result of links contain the name</returns>
        [HttpGet]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<LinkSearchViewModel> GetLinkSearchByName(
            [FromQuery] string name,
            long timestamp
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var linkViewModels = linksService.GetLinksByName(name, userId);

            var linkSearchViewModel = new LinkSearchViewModel
            {
                Timestamp = timestamp,
                Links = linkViewModels,
            };

            return Ok(linkSearchViewModel);
        }

        /// <summary>
        /// Create a new link
        /// </summary>
        /// <param name="newLink">new link detail</param>
        /// <returns>the new link</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<LinkViewModel>> PostNewLink(
            [FromBody] LinkPostViewModel newLink
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var invalidParams = linksService.ValidatePost(newLink);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            // validate link
            var isLinkValid = UrlUtils.IsValidUrlFormat(newLink.Url);
            if (!isLinkValid)
            {
                return BadRequest(Messages.LinkInvalid);
            }

            var linkViewModel = await linksService.PostNewLinkAsync(userId, newLink);
            return Ok(linkViewModel);
        }

        /// <summary>
        /// Update a link
        /// </summary>
        /// <param name="id">link id</param>
        /// <param name="link">link detail to be updated</param>
        /// <returns>the updated link</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.LINKS)]
        public async Task<ActionResult<LinkViewModel>> PatchLink(
            int id,
            [FromBody] LinkPatchViewModel link
        )
        {
            var invalidParams = linksService.ValidatePatch(link);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            // validate link
            if (link.Url != null)
            {
                var isLinkValid = UrlUtils.IsValidUrlFormat(link.Url);
                if (!isLinkValid)
                {
                    return BadRequest(Messages.LinkInvalid);
                }
            }

            var linkViewModel = await linksService.PatchLinkAsync(id, link);
            return Ok(linkViewModel);
        }
    }
}
