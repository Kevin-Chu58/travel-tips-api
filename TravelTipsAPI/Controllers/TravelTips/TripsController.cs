using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Firebase;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_image;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Controllers.TravelTips
{
    /// <summary>
    /// The controller of Trips
    /// </summary>
    /// <param name="tripsService">trips service</param>
    /// <param name="imagesService">images service</param>
    [Route("api/[controller]")]
    public class TripsController(ITripsService tripsService, IImagesService imagesService)
        : TravelTipsControllerBase
    {
        /// <summary>
        /// Get trips by title
        /// </summary>
        /// <param name="title">the title of the trips</param>
        /// <returns>a list of trips that includes the title</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<TripViewModel>> GetTripsByTitle([FromQuery] string title)
        {
            var tripViewModels = tripsService.GetTripsByTitle(title);
            return Ok(tripViewModels);
        }

        /// <summary>
        /// Get a public trip by its id
        /// </summary>
        /// <param name="id">the id of a trip</param>
        /// <returns>a trip with that id, Not Found otherwise</returns>
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public ActionResult<TripViewModel> GetPublicTripById(int id)
        {
            try
            {
                var tripViewModel = tripsService.GetTripByTripId(id);
                return Ok(tripViewModel);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
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
        public ActionResult<TripViewModel> GetTripById(int id)
        {
            var tripViewModel = tripsService.GetTripByTripId(id);
            return Ok(tripViewModel);
        }

        /// <summary>
        /// Post a new trip to db
        /// </summary>
        /// <param name="name">a new trip title</param>
        /// <returns>the new trip posted to db</returns>
        [HttpPost]
        [Route("{name}")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<TripViewModel>> PostNewTrip(string name)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // validate the trip name
            var invalidParams = tripsService.ValidatePost(name);
            if (invalidParams.Count > 0)
            {
                var invalidInputs = string.Join(", ", invalidParams);
                return BadRequest(string.Format(Messages.InputInvalid, invalidInputs));
            }

            var tripViewModel = await tripsService.PostNewTripAsync(userId, name);
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
        public async Task<ActionResult<TripPatchViewModel>> PatchTrip(
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

            var tripPatchViewModel = await tripsService.PatchTripAsync(trip, tripPatch);
            return Ok(tripPatchViewModel);
        }

        /// <summary>
        /// Make the trip public or private
        /// </summary>
        /// <param name="isPublic">the published status</param>
        /// <param name="tripIds">the ids of the list of trips</param>
        /// <returns>a trip with updated published status</returns>
        [HttpPatch]
        [Route("isPublic/{isPublic}")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<List<int>>> UpdateTripIsPublic(
            bool isPublic,
            [FromBody] int[] tripIds
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // verify is the owner of all trip ids
            var isOwnerList = tripsService.IsOwnerList(userId, tripIds);
            if (!isOwnerList)
            {
                return BadRequest(Messages.TripUnauthorized);
            }

            var _tripIds = await tripsService.UpdateIsPublicAsync(tripIds, isPublic);
            return Ok(_tripIds);
        }

        /// <summary>
        /// Make the trip trashed or untrashed
        /// </summary>
        /// <param name="isHidden">the trashed status</param>
        /// <param name="tripIds">the ids of the list of trips</param>
        /// <returns>a trip with updated trashed status</returns>
        [HttpPatch]
        [Route("isHidden/{isHidden}")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<List<int>>> UpdateTripIsHidden(
            bool isHidden,
            [FromBody] int[] tripIds
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // verify is the owner of all trip ids
            var isOwnerList = tripsService.IsOwnerList(userId, tripIds);
            if (!isOwnerList)
            {
                return BadRequest(Messages.TripUnauthorized);
            }

            var _tripIds = await tripsService.UpdateIsHiddenAsync(tripIds, isHidden);
            return Ok(_tripIds);
        }

        /// <summary>
        /// Get a list of images by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>a list of images attached to the trips</returns>
        [HttpGet]
        [Route("{id}/images")]
        [SetUserId]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ImageViewModel>>> GetImagesByTripId(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                var trip = tripsService.FindTripByParams(id);

                if (trip.IsPublic == false && userId != trip.CreatedBy)
                {
                    return BadRequest(Messages.TripUnauthorized);
                }

                var imageIds = imagesService.GetImageIdsByTripId(id).ToArray();

                var imageViewModels = await imagesService.GetImagesByIds(imageIds);

                return Ok(imageViewModels);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// attach image to a trip
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="imageId">iamge id</param>
        /// <returns>an image relation with trip</returns>
        [HttpPost]
        [Route("{id}/image/{imageId}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<ImageRelationViewModel>> AttachImage(int id, int imageId)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // validate the user ownership on the image
            var ownership = imagesService.IsOwner(userId, imageId);

            if (!ownership)
                return Forbid(Messages.ImageUnauthorized);

            // check if maximum 4 images have already been attached to the tip
            var maxCount = 4;
            var imageCount = imagesService.GetImageIdsByTripId(id).Count();

            if (imageCount >= maxCount)
            {
                return Forbid(Messages.ImageMaxAttached);
            }

            var imageRelation = await imagesService.AttachImageToTrip(imageId, id);

            return Ok(imageRelation);
        }

        /// <summary>
        /// detach image from a trip
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="imageId">image id</param>
        /// <returns>the removed image relation with trip</returns>
        [HttpDelete]
        [Route("{id}/image/{imageId}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<ImageRelationViewModel>> DetachImage(int id, int imageId)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // validate the user ownership on the image
            var ownership = imagesService.IsOwner(userId, imageId);

            if (!ownership)
                return Forbid(Messages.ImageUnauthorized);

            try
            {
                var tripImage = await imagesService.DetachImageFromTrip(imageId, id);
                return Ok(tripImage);
            }
            catch (Exception ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
