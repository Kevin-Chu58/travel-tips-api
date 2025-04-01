using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Controllers
{
    [Route("api/[controller]")]
    public class SmallTripsController(IUsersService usersService, ITripsService tripsService, ISmallTripsService smallTripsService) : Controller
    {
        [HttpGet]
        [Route("{tripId}")]
        public ActionResult<IEnumerable<SmallTripViewModel>> GetSmallTripsByTripId(int tripId)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

            var currentUser = usersService.GetUserByUserId(userId);
            var isOwner = tripsService.IsOwner(currentUser.Id, tripId);

            if (isOwner)
            {
                var smallTripViewModels = smallTripsService.GetSmallTripsByTripId(tripId);
                return Ok(smallTripViewModels);
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("{tripId}")]
        public async Task<ActionResult<SmallTripViewModel>> PostNewSmallTrip(int tripId, [FromBody] SmallTripPostViewModel newSmallTrip)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

            var currentUser = usersService.GetUserByUserId(userId);
            var isOwner = tripsService.IsOwner(currentUser.Id, tripId);

            if (isOwner)
            {
                var smallTripViewModel = await smallTripsService.PostNewSmallTripsAsync(tripId, newSmallTrip);
                return Ok(smallTripViewModel);
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPatch]
        [Route("{id}")]
        public async Task<ActionResult<SmallTripViewModel>> PatchSmallTripAsync(int id, [FromBody] TripPatchViewModel smallTripPatch)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

            var smallTrip = smallTripsService.GetSmallTripById(id);
            if (smallTrip == null)
                return NotFound("Small Trip not found.");

            var currentUser = usersService.GetUserByUserId(userId);
            var isOwner = tripsService.IsOwner(currentUser.Id, smallTrip.TripId);

            if (isOwner)
            {
                var smallTripViewModel = await smallTripsService.PatchSmallTripAsync(id, smallTripPatch);
                return Ok(smallTripViewModel);
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
