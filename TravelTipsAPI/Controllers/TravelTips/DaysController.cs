using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
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
        ITripSharesService tripSharesService,
        IDaysService daysService
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
        public ActionResult<IEnumerable<DayViewModel>> GetDaysById(int tripId)
        {
            var trip = tripsService.FindTripByParams(tripId);

            if (trip is null)
                return NotFound(Messages.TripNotFound);

            // check if the trip is either public or the user is the owner or shared user
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var isShared = tripSharesService.IsTripSharedWithUser(trip.Id, userId);

            var isRestricted = trip.CreatedBy == userId || isShared;

            if ((!trip.IsPublic && !isRestricted) || trip.IsHidden)
                return BadRequest(Messages.TripUnauthorized);

            var dayViewModels = daysService.GetDaysByTripId(tripId);

            return Ok(dayViewModels);
        }

        /// <summary>
        /// Create a new day by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>the new day</returns>
        [HttpPost]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<DayViewModel>> PostNewDay(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var isEditable = tripsService.CanUserEditTrip(id, userId);
            if (!isEditable)
                return BadRequest(Messages.MembershipRequired);

            // check day max restriction
            var dayViewModels = daysService.GetDaysByTripId(id);

            if (dayViewModels.Count() >= NumberConstraints.MAX_DAY_PER_TRIP)
                return BadRequest(Messages.DayMaxReached);

            try
            {
                var newDay = await daysService.PostNewDayAsync(userId, id);

                return Ok(newDay);
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
        //[HttpPatch]
        //[Route("{id}")]
        //[IsOwner(Resource = Resources.DAYS)]
        //public async Task<ActionResult<DayViewModel>> UpdateDay(
        //    int id,
        //    [FromBody] DayPatchViewModel dayPatch
        //)
        //{
        //    try
        //    {
        //        Day day = daysService.FindDayById(id);

        //        var updatedDayViewModel = await daysService.PatchDayAsync(day, dayPatch);

        //        return Ok(updatedDayViewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        /// <summary>
        /// Delete a day by its id
        /// </summary>
        /// <param name="id">day id</param>
        /// <returns>the day deleted</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.DAYS)]
        public async Task<ActionResult> DeleteDay(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            Day day = daysService.FindDayById(id);

            var isEditable = tripsService.CanUserEditTrip(day.TripId, userId);
            if (!isEditable)
                return BadRequest(Messages.MembershipRequired);

            await daysService.DeleteDay(day);

            return Ok();
        }
    }
}
