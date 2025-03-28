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

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<TripViewModel>> PostNewTrip([FromBody] TripPostViewModel newTrip)
        {
            var tripViewModel = await tripsService.PostNewTripAsync(newTrip);
            return CreatedAtAction(nameof(PostNewTrip), new { tripViewModel?.Id }, tripViewModel);
        }

        [HttpPatch]
        [Route("{id}/isPublic")]
        public async Task<ActionResult<TripViewModel>> UpdateTripIsPublic(int id, [FromBody] bool isPublic)
        {
            ClaimsPrincipal user = HttpContext.User;
            string? userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
    }
}
