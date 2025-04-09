using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
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
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<DayViewModel>> PostNewDay(int tripId, [FromBody] DayPostViewModel newDay)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var yourTripIds = tripsService.GetYourTripIds(userId);
            if (!yourTripIds.Any(id => id == tripId))
                return Unauthorized("You are not authorized.");

            var dayViewModel = await daysService.PostNewDayAsync(tripId, userId, newDay);

            return Ok(dayViewModel);
        }

        /// <summary>
        /// Update a day with day details
        /// </summary>
        /// <param name="id">day id</param>
        /// <param name="day">day details to be updated</param>
        /// <returns>the updated day</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.DAYS)]
        public async Task<ActionResult<DayViewModel>> UpdateDay(int id, [FromBody] DayPatchViewModel day)
        {
            var updatedDayViewModel = await daysService.PatchDayAsync(id, day);

            return Ok(updatedDayViewModel);
        }
    }
}
