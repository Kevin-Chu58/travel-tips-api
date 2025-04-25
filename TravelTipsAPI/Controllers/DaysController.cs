using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Days
    /// </summary>
    /// <param name="usersService"></param>
    /// <param name="tripsService"></param>
    /// <param name="daysService"></param>
    [Route("api/[controller]")]
    public class DaysController(ITripsService tripsService, IDaysService daysService)
        : TravelTipsControllerBase
    {
        /// <summary>
        /// Get the days by trip id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns>the days under the trip</returns>
        [HttpGet]
        [Route("{tripId}")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<DayViewModel>> GetDaysById(int tripId)
        {
            try
            {
                var dayViewModels = daysService.GetDaysByTripId(tripId);

                return Ok(dayViewModels);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Create a new day with day detail and trip id
        /// </summary>
        /// <param name="newDay">new day detail</param>
        /// <returns>the new day</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<DayViewModel>> PostNewDay([FromBody] DayPostViewModel newDay)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // verify the ownership of the parent trip
            var yourTripIds = tripsService.GetYourTripIds(userId);
            if (!yourTripIds.Any(id => id == newDay.TripId))
                return Unauthorized(Messages.AccessDenied);

            try
            {
                var dayViewModel = await daysService.PostNewDayAsync(userId, newDay);

                return Ok(dayViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update a day with day details
        /// </summary>
        /// <param name="id">day id</param>
        /// <param name="dayPatch">day details to be updated</param>
        /// <returns>the updated day</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.DAYS)]
        public async Task<ActionResult<DayViewModel>> UpdateDay(
            int id,
            [FromBody] DayPatchViewModel dayPatch
        )
        {
            Day day = daysService.FindDayById(id);

            try
            {
                var updatedDayViewModel = await daysService.PatchDayAsync(day, dayPatch);

                return Ok(updatedDayViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
