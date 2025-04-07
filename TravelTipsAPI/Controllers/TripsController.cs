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
    /// The controller of Trips
    /// </summary>
    /// <param name="usersService">users service</param>
    /// <param name="tripsService">trips service</param>
    [Route("api/[controller]")]
    public class TripsController(IUsersService usersService, ITripsService tripsService, ISmallTripsService smallTripsService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a trip by its id
        /// </summary>
        /// <param name="id">the id of a trip</param>
        /// <returns>a trip with that id</returns>
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public ActionResult<TripDetailViewModel> GetTripById(int id)
        {
            var tripViewModel = tripsService.GetTripById(id, true);
            if (tripViewModel == null)
                return NotFound();

            var smallTripViewModels = smallTripsService.GetSmallTripsByTripId(tripViewModel.Id);

            var tripDetailViewModel = new TripDetailViewModel
            {
                Id = tripViewModel.Id,
                Name = tripViewModel.Name,
                Description = tripViewModel.Description,
                CreatedBy = tripViewModel.CreatedBy,
                CreatedAt = tripViewModel.CreatedAt,
                LastUpdatedAt = tripViewModel.LastUpdatedAt,
                SmallTrips = smallTripViewModels,
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
        [Route("yours")]
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetYourTrips()
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

            var currentUser = await usersService.GetUserByUserId(userId);

            var yourTripViewModels = tripsService.GetYourTrips(currentUser.Id);
            return Ok(yourTripViewModels);
        }

        /// <summary>
        /// Post a new trip to db
        /// </summary>
        /// <param name="newTrip">a new trip to be posted</param>
        /// <returns>the new trip posted to db</returns>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult<TripViewModel>> PostNewTrip([FromBody] TripPostViewModel newTrip)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return NotFound("User not found.");

            if (string.IsNullOrEmpty(newTrip.Name.Trim())) 
                return BadRequest("Name cannot be null or empty.");

            UserViewModel user = await usersService.GetUserByUserId(userId);

            var tripViewModel = await tripsService.PostNewTripAsync(user.Id, newTrip);
            return CreatedAtAction(nameof(PostNewTrip), new { tripViewModel?.Id }, tripViewModel);
        }

        /// <summary>
        /// Update a trip's information
        /// </summary>
        /// <param name="id">the id of the trip</param>
        /// <param name="tripPatchViewModel">the trip information to be updated</param>
        /// <returns>the updated trip</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<TripViewModel>> PatchTrip(int id, [FromBody] TripPatchViewModel trip)
        {
                var tripViewModel = await tripsService.PatchTripAsync(id, trip);
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
        public async Task<ActionResult<TripViewModel>> UpdateTripIsPublic(int id, [FromBody] bool isPublic)
        {
                var tripViewModel = await tripsService.UpdateIsPublicAsync(id, isPublic);
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
        public async Task<ActionResult<TripViewModel>> UpdateTripIsHidden(int id, [FromBody] bool isHidden)
        {
                var tripViewModel = await tripsService.UpdateIsHiddenAsync(id, isHidden);
                return Ok(tripViewModel);
        }
    }
}
