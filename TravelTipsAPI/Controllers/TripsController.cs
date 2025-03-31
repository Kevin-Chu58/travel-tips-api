using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.Services;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Trips
    /// </summary>
    /// <param name="usersService">users service</param>
    /// <param name="tripsService">trips service</param>
    [Route("api/[controller]")]
    public class TripsController(IUsersService usersService, ITripsService tripsService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a trip by its id
        /// </summary>
        /// <param name="id">the id of a trip</param>
        /// <returns>a trip with that id</returns>
        [HttpGet]
        [Route("{id}")]
        public ActionResult<TripViewModel> GetTripById(int id)
        {
            var tripViewModel = tripsService.GetTripById(id);
            return Ok(tripViewModel);
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
            var tripViewModels = tripsService.GetTripByName(name);
            return Ok(tripViewModels);
        }

        /// <summary>
        /// Get your own trips
        /// </summary>
        /// <returns>a list of your own trips</returns>
        [HttpGet]
        [Route("yours")]
        public ActionResult<IEnumerable<TripViewModel>> GetYourTrips()
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

            var currentUser = usersService.GetUserByUserId(userId);

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

            if (string.IsNullOrEmpty(newTrip.Name)) 
                return BadRequest("Name cannot be null or empty.");

            UserViewModel user = await usersService.GetUserByUserId(userId);

            var tripViewModel = await tripsService.PostNewTripAsync(newTrip, user.Id);
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
        public async Task<ActionResult<TripViewModel>> PatchTrip(int id, [FromBody] TripPatchViewModel tripPatchViewModel)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

            var currentUser = usersService.GetUserByUserId(userId);

            var isOwner = tripsService.IsOwner(currentUser.Id, id);
            if (isOwner)
            {
                var tripViewModel = await tripsService.PatchTripAsync(id, tripPatchViewModel);
                return Ok(tripViewModel);
            }
            else
                return Unauthorized();
        }

        /// <summary>
        /// Make the trip public or private
        /// </summary>
        /// <param name="id">the id of the trip</param>
        /// <param name="isPublic">the published status</param>
        /// <returns>a trip with updated published status</returns>
        [HttpPatch]
        [Route("{id}/isPublic")]
        public async Task<ActionResult<TripViewModel>> UpdateTripIsPublic(int id, [FromBody] bool isPublic)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

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

        /// <summary>
        /// Make the trip trashed or untrashed
        /// </summary>
        /// <param name="id">the id of the trip</param>
        /// <param name="isHidden">the trashed status</param>
        /// <returns>a trip with updated trashed status</returns>
        [HttpPatch]
        [Route("{id}/isHidden")]
        public async Task<ActionResult<TripViewModel>> UpdateTripIsHidden(int id, [FromBody] bool isHidden)
        {
            // Get Auth0 UserId
            string? userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User not found.");

            var currentUser = usersService.GetUserByUserId(userId);

            var isOwner = tripsService.IsOwner(currentUser.Id, id);
            if (isOwner)
            {
                var tripViewModel = await tripsService.UpdateIsHiddenAsync(id, isHidden);
                return Ok(tripViewModel);
            }
            else
                return Unauthorized();
        }
    }
}
