using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Days
    /// </summary>
    /// <param name="usersService"></param>
    /// <param name="tripsService"></param>
    /// <param name="daysService"></param>
    [Route("api/[controller]")]
    public class DaysController(IUsersService usersService, ITripsService tripsService, IDaysService daysService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get the days by trip id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns>the days under the trip</returns>
        [HttpGet]
        [Route("{tripId}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<DayViewModel>> GetDayById(int tripId)
        {
            var dayViewModels = daysService.GetDaysByTripId(tripId);

            if (dayViewModels == null)
                return NotFound();

            return Ok(dayViewModels);
        }

        /// <summary>
        /// Create a new day with day detail and trip id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <param name="newDay">new day detail</param>
        /// <returns>the new day</returns>
        [HttpPost]
        [Route("{tripId}")]
        public async Task<ActionResult<DayViewModel>> PostNewDay(int tripId, [FromBody] DayPostViewModel newDay)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return NotFound("User not found.");

            var tripViewModel = tripsService.GetTripById(tripId);

            if (tripViewModel == null)
                return NotFound("Trip not found.");

            var user = usersService.GetUserByUserId(userId);
            var isOwner = tripsService.IsOwner(user.Id, tripId);
            if (!isOwner)
                return Unauthorized("You are not authorized.");

            var dayViewModel = await daysService.PostNewDayAsync(tripId, newDay);

            return Ok(dayViewModel);
        }

        [HttpPatch]
        [Route("{id}")]
        public async Task<ActionResult<DayViewModel>> UpdateDay(int id, [FromBody] DayPatchViewModel day)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return NotFound("User not found.");

            var dayViewModel = daysService.GetDayById(id);

            if (dayViewModel == null)
                return NotFound("Day not found");

            var tripViewModel = tripsService.GetTripById(dayViewModel.TripId);

            if (tripViewModel == null)
                return NotFound("Trip not found.");

            var user = usersService.GetUserByUserId(userId);
            var isOwner = tripsService.IsOwner(user.Id, dayViewModel.TripId);
            if (!isOwner)
                return Unauthorized("You are not authorized.");

            var updatedDayViewModel = await daysService.PatchDayAsync(id, day);

            return Ok(updatedDayViewModel);
        }
    }
}
