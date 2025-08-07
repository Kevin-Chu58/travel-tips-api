using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Services.TravelTipsServices;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Controllers.TravelTips
{
    /// <summary>
    /// The controller of Days
    /// </summary>
    /// <param name="tripsService">trips service</param>
    /// <param name="daysService">days service</param>
    [Route("api/[controller]")]
    public class DaysController(
        ITripsService tripsService,
        IDaysService daysService,
        ITripAttractionOrdersService taosService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get the days by trip id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns>the days under the trip</returns>
        [HttpGet]
        [Route("{tripId}")]
        [AllowAnonymous]
        [SetUserId]
        public ActionResult<IEnumerable<DayViewModel>> GetDaysById(int tripId)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                // check if the trip is either public or the user is the owner
                var trip = tripsService.FindTripByParams(tripId);

                if (trip.IsPublic == false && userId != trip.CreatedBy)
                {
                    return Forbid(Messages.DayUnauthorized);
                }

                var dayViewModels = daysService.GetDaysByTripId(tripId);

                return Ok(dayViewModels);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Create a new day by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="title">day title</param>
        /// <returns>the new day</returns>
        [HttpPost]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<DayViewModel>> PostNewDay(int id, [FromBody] string? title)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                var dayViewModel = await daysService.PostNewDayAsync(userId, id, title);

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
            try
            {
                // validate the inputs
                if (dayPatch.Title?.Length > 50)
                    return BadRequest(Messages.DayInputInvalid);

                Day day = daysService.FindDayById(id);

                var updatedDayViewModel = await daysService.PatchDayAsync(day, dayPatch);

                return Ok(updatedDayViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a day by its id
        /// </summary>
        /// <param name="id">day id</param>
        /// <returns>the day deleted</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.DAYS)]
        public async Task<ActionResult<DayViewModel>> DeleteDay(int id)
        {
            _ = taosService.DeleteTaosByDayId(id);

            Day day = daysService.FindDayById(id);
            var dayViewModel = await daysService.DeleteDay(day);

            return Ok(dayViewModel);
        }
    }
}
