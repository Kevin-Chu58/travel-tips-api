using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
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
    /// The controller of Trips
    /// </summary>
    /// <param name="tripsService">trips service</param>
    /// <param name="smallTripsService"></param>small trips service</param>
    [Route("api/[controller]")]
    public class TripsController(
        ITripsService tripsService,
        IDaysService daysService,
        ITripAttractionOrdersService tripAttractionOrdersService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a public trip by its id
        /// </summary>
        /// <param name="id">the id of a trip</param>
        /// <returns>a trip with that id, Not Found otherwise</returns>
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public ActionResult<TripDetailViewModel> GetPublicTripById(int id)
        {
            Trip trip;
            try
            {
                trip = tripsService.FindTripByParams(id, true);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            TripViewModel tripViewModel = (TripViewModel)trip;

            var days = daysService.GetDaysByTripId(id);

            foreach (var day in days)
            {
                var taos = tripAttractionOrdersService.GetTripAttractionOrdersByDayId(day.Id);
                var taoViewModels = new List<TripAttractionOrderViewModel>();

                foreach (var tao in taos)
                {
                    taoViewModels.Add(tripAttractionOrdersService.ToViewModel(tao));
                }

                day.TripAttractionOrderCount = taos.Count();
                day.TripAttractionOrders = taoViewModels;
            }

            var tripDetailViewModel = new TripDetailViewModel
            {
                Id = tripViewModel.Id,
                Name = tripViewModel.Name,
                Description = tripViewModel.Description,
                CreatedBy = tripViewModel.CreatedBy,
                CreatedAt = tripViewModel.CreatedAt,
                LastUpdatedAt = tripViewModel.LastUpdatedAt,
                Days = days,
            };

            return Ok(tripDetailViewModel);
        }

        /// <summary>
        /// Get trips by name
        /// </summary>
        /// <param name="name">the name of the trips</param>
        /// <returns>a list of trips that includes the name</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<TripViewModel>> GetTripsByName([FromQuery] string name)
        {
            var tripViewModels = tripsService.GetTripsByName(name);
            return Ok(tripViewModels);
        }

        /// <summary>
        /// Get your own trips
        /// </summary>
        /// <returns>a list of your own trips</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<IEnumerable<TripViewModel>> GetYourTrips()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var myTripViewModels = tripsService.GetTripsByUserId(userId);
            return Ok(myTripViewModels);
        }

        /// <summary>
        /// Get your own trip by id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>the trip you own</returns>
        [HttpGet]
        [Route("my/{id}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public ActionResult<TripDetailViewModel> GetTripById(int id)
        {
            Trip trip = tripsService.FindTripByParams(id);
            var tripViewModel = (TripViewModel)trip;

            var days = daysService.GetDaysByTripId(id);

            foreach (var day in days)
            {
                var taos = tripAttractionOrdersService.GetTripAttractionOrdersByDayId(day.Id);
                var taoViewModels = new List<TripAttractionOrderViewModel>();

                foreach (var tao in taos)
                {
                    taoViewModels.Add(tripAttractionOrdersService.ToViewModel(tao));
                }

                day.TripAttractionOrderCount = taos.Count();
                day.TripAttractionOrders = taoViewModels;
            }

            var tripDetailViewModel = new TripDetailViewModel
            {
                Id = tripViewModel.Id,
                Name = tripViewModel.Name,
                Description = tripViewModel.Description,
                CreatedBy = tripViewModel.CreatedBy,
                CreatedAt = tripViewModel.CreatedAt,
                LastUpdatedAt = tripViewModel.LastUpdatedAt,
                Days = days,
            };

            return Ok(tripDetailViewModel);
        }

        /// <summary>
        /// Post a new trip to db
        /// </summary>
        /// <param name="newTrip">a new trip to be posted</param>
        /// <returns>the new trip posted to db</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<TripViewModel>> PostNewTrip(
            [FromBody] TripPostViewModel newTrip
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // validate the inputs
            var invalidParams = tripsService.ValidatePost(newTrip);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            var tripViewModel = await tripsService.PostNewTripAsync(userId, newTrip);
            return CreatedAtAction(nameof(PostNewTrip), new { tripViewModel?.Id }, tripViewModel);
        }

        /// <summary>
        /// Update a trip's information
        /// </summary>
        /// <param name="id">the id of the trip</param>
        /// <param name="tripPatch">the trip information to be updated</param>
        /// <returns>the updated trip</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<TripViewModel>> PatchTrip(
            int id,
            [FromBody] TripPatchViewModel tripPatch
        )
        {
            Trip trip;
            try
            {
                trip = tripsService.FindTripByParams(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            // validate the inputs
            var invalidParams = tripsService.ValidatePatch(tripPatch);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            var tripViewModel = await tripsService.PatchTripAsync(trip, tripPatch);
            return Ok(tripViewModel);
        }

        /// <summary>
        /// Make the trip public or private
        /// </summary>
        /// <param name="id">the id of the trip</param>
        /// <param name="isPublic">the published status</param>
        /// <returns>a trip with updated published status</returns>
        [HttpPatch]
        [Route("{id}/isPublic")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<TripViewModel>> UpdateTripIsPublic(
            int id,
            [FromBody] bool isPublic
        )
        {
            Trip trip;
            try
            {
                trip = tripsService.FindTripByParams(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            var tripViewModel = await tripsService.UpdateIsPublicAsync(trip, isPublic);
            return Ok(tripViewModel);
        }

        /// <summary>
        /// Make the trip trashed or untrashed
        /// </summary>
        /// <param name="id">the id of the trip</param>
        /// <param name="isHidden">the trashed status</param>
        /// <returns>a trip with updated trashed status</returns>
        [HttpPatch]
        [Route("{id}/isHidden")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<TripViewModel>> UpdateTripIsHidden(
            int id,
            [FromBody] bool isHidden
        )
        {
            Trip trip;
            try
            {
                trip = tripsService.FindTripByParams(id);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            var tripViewModel = await tripsService.UpdateIsHiddenAsync(trip, isHidden);
            return Ok(tripViewModel);
        }
    }
}
