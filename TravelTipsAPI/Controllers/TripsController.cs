using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Controllers
{
    [Route("api/[controller]")]
    public class TripsController(IUsersService usersService, ITripsService tripsService) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("{id}")]
        public ActionResult<TripViewModel> GetTripById(int id)
        {
            var tripViewModel = tripsService.GetTripById(id);
            return Ok(tripViewModel);
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<TripViewModel>> GetTripsByName([FromQuery] string name)
        {
            // TODO
            var res = new List<TripViewModel>();
            return Ok(res);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<TripViewModel>> PostNewTrip([FromBody] TripPostViewModel newTrip)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            UserViewModel user = await usersService.GetUserByUserId(userId);

            var tripViewModel = await tripsService.PostNewTripAsync(newTrip, user.Id);
            return CreatedAtAction(nameof(PostNewTrip), new { tripViewModel?.Id }, tripViewModel);
        }

        [HttpPatch]
        [Route("{id}/isPublic")]
        public async Task<ActionResult<TripViewModel>> UpdateTripIsPublic(int id, [FromBody] bool isPublic)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var currentUser = usersService.GetUserByUserId(userId);

            var isOwner = tripsService.IsOwner(currentUser.Id, id);
            if (isOwner)
            {
                var tripViewModel = await tripsService.UpdateIsPublicAsync(id, isPublic);
                return Ok(tripViewModel);
            }
            else
                return Unauthorized();
        }

        [HttpPatch]
        [Route("{id}/isHidden")]
        public async Task<ActionResult<TripViewModel>> UpdateTripIsHidden(int id, [FromBody] bool isHidden)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound();

            var currentUser = usersService.GetUserByUserId(userId);

            var isOwner = tripsService.IsOwner(currentUser.Id, id);
            if (isOwner)
            {
                var tripViewModel = await tripsService.UpdateIsHiddenAsync(id, isHidden);
                return Ok(tripViewModel);
            }
            else
                return Unauthorized();
        }
    }
}
