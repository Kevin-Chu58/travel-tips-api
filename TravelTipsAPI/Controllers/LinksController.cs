using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Links
    /// </summary>
    [Route("api/[controller]")]
    public class LinksController(IUsersService usersService, ILinksService linksService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get the link search result by name
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="timeStamp">the time when the http request created</param>
        /// <returns>a link search result of links contain the name</returns>
        [HttpGet]
        [Route("")]
        public ActionResult<LinkSearchViewModel> GetLinkSearchByName([FromQuery] string name, int timeStamp)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return NotFound("User not found.");

            var user = usersService.GetUserByUserId(userId);
            
            var linkSearchViewModel = linksService.GetLinksByName(timeStamp, name, user.Id);
            return Ok(linkSearchViewModel);
        }

        /// <summary>
        /// Create a new link
        /// </summary>
        /// <param name="newLink">new link detail</param>
        /// <returns>the new link</returns>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult<LinkViewModel>> PostNewLink([FromBody] LinkPostViewModel newLink)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return NotFound("User not found.");

            var user = usersService.GetUserByUserId(userId);

            var linkViewModel = await linksService.PostNewLinkAsync(user.Id, newLink);
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
        public async Task<ActionResult<LinkViewModel>> PatchLink(int id, [FromBody] LinkPatchViewModel link)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return NotFound("User not found.");

            var user = usersService.GetUserByUserId(userId);
            var isOwner = linksService.IsOwner(user.Id, id);
            if (!isOwner)
                return Unauthorized();

            var linkViewModel = await linksService.PatchLinkAsync(id, link);
            return Ok(linkViewModel);
        }
    }
}
