using System.Security.Claims;
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
        IDaysService daysService
    //ITripAttractionOrdersService taosService
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
            var myTripIds = tripsService.GetMyTripIds(userId);
            if (!myTripIds.Any(id => id == newDay.TripId))
                return Unauthorized(Messages.AccessDenied);

            // validate the inputs
            var invalidParams = daysService.ValidatePost(newDay);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

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

            // validate the inputs
            var invalidParams = daysService.ValidatePatch(dayPatch);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

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

        ///// <summary>
        ///// Delete a day by its id
        ///// </summary>
        ///// <param name="id">day id</param>
        ///// <returns>the day deleted</returns>
        //[HttpDelete]
        //[Route("{id}")]
        //[IsOwner(Resource = Resources.DAYS)]
        //public async Task<ActionResult<DayViewModel>> DeleteDay(int id)
        //{
        //    var taos = taosService.GetTripAttractionOrdersByDayId(id);

        //    var taoViewModels = new List<TripAttractionOrderViewModel>();
        //    foreach (var t in taos)
        //    {
        //        taoViewModels.Add(await taosService.DeleteTripAttractionOrderAsync(t));
        //    }

        //    Day day = daysService.FindDayById(id);
        //    var dayViewModel = await daysService.DeleteDay(day);
        //    dayViewModel.TripAttractionOrders = taoViewModels;

        //    return Ok(dayViewModel);
        //}
    }
}
