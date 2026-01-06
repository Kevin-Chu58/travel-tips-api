using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.db_search;
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
    public class TripsController(
        ITripsService tripsService,
        IDaysService daysService,
        ITripAttractionOrdersService taosService,
        IImagesService imagesService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get trips by title
        /// </summary>
        /// <param name="title">the title of the trips</param>
        /// <returns>a list of trips that includes the title</returns>
        [HttpGet]
        [Route("")]
        //[AllowAnonymous]
        [IsOwner(Resource = Resources.NONE)] // requires login as personal project
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetTripsByTitle(
            [FromQuery] string title
        )
        {
            var tripViewModels = tripsService.GetTripsByTitle(title);

            tripViewModels = await AppendImagesToTripsAsync(tripViewModels);

            return Ok(tripViewModels);
        }

        /// <summary>
        /// Get a public trip by its id
        /// </summary>
        /// <param name="id">the id of a trip</param>
        /// <returns>a trip with that id, Not Found otherwise</returns>
        [HttpGet]
        [Route("{id}")]
        //[AllowAnonymous]
        //[SetUserId]
        [IsOwner(Resource = Resources.NONE)] // requires login as personal project
        public async Task<ActionResult<TripViewModel>> GetTripById(int id)
        {
            try
            {
                var tripViewModel = tripsService.GetTripByTripId(id);

                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                if (tripViewModel.IsPublic || tripViewModel.CreatedBy!.Id == userId)
                {
                    tripViewModel.Images = await GetImagesByTripIdAsync(tripViewModel.Id);
                    return Ok(tripViewModel);
                }
                else
                    return NotFound(Messages.TripNotFound);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get my own trips
        /// </summary>
        /// <returns>a list of my own trips</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetMyTrips()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var myTripViewModels = tripsService.GetTripsByUserId(userId);
            myTripViewModels = await AppendImagesToTripsAsync(myTripViewModels);
            return Ok(myTripViewModels);
        }

        /// <summary>
        /// Get TaoGeo trip list by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>the TaoGeo trip list</returns>
        [HttpGet]
        [Route("{id}/day-overview")]
        //[AllowAnonymous]
        //[SetUserId]
        [IsOwner(Resource = Resources.NONE)] // requires login as personal project
        public ActionResult<IEnumerable<TripAttractionOrderGeoViewModel>> GetTaoGeosById(int id)
        {
            try
            {
                // check the trip is either public or the user is the owner
                var tripViewModel = tripsService.GetTripByTripId(id);

                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                if (tripViewModel.IsPublic || tripViewModel.CreatedBy!.Id == userId)
                {
                    var geoTripList = taosService.GetTaoGeosByTripId(id);

                    return Ok(geoTripList);
                }
                else
                    return NotFound(Messages.TripNotFound);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
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
        /// Update trip region id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="regionId">new region id</param>
        /// <returns>the updated complete region</returns>
        [HttpPatch]
        [Route("{id}/region")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<RegionCompleteViewModel?>> UpdateTripRegion(
            int id,
            [FromBody] int? regionId
        )
        {
            try
            {
                var trip = tripsService.FindTripByParams(id);
                if (trip is null)
                    return NotFound(Messages.TripNotFound);

                var regionComplete = await tripsService.UpdateRegionAsync(trip, regionId);
                return Ok(regionComplete);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update trip budget
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="budget">budget</param>
        /// <returns>the updated budget</returns>
        [HttpPatch]
        [Route("{id}/budget")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<int?>> UpdateTripBudget(int id, [FromBody] int? budget)
        {
            try
            {
                var trip = tripsService.FindTripByParams(id);
                if (trip is null)
                    return NotFound(Messages.TripNotFound);

                var newBudget = await tripsService.UpdateBudgetAsync(trip, budget);
                return Ok(newBudget);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get a list of images by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>a list of images attached to the trips</returns>
        [HttpGet]
        [Route("{id}/images")]
        [AllowAnonymous]
        [SetUserId]
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

                var images = await GetImagesByTripIdAsync(id);

                return Ok(images);
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
        public async Task<ActionResult<ImageViewModel>> AttachImage(int id, int imageId)
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

            await imagesService.AttachImageToTrip(imageId, id);

            var imageViewModels = await imagesService.GetImagesByIds([imageId]);

            return Ok(imageViewModels.First());
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

        private async Task<IEnumerable<TripViewModel>> AppendImagesToTripsAsync(
            IEnumerable<TripViewModel> trips
        )
        {
            var tripList = trips.ToList();

            var tasks = tripList.Select(async trip =>
            {
                trip.Images = await GetImagesByTripIdAsync(trip.Id);
            });

            // Wait for all
            await Task.WhenAll(tasks);

            return tripList;
        }

        private async Task<IEnumerable<ImageViewModel>> GetImagesByTripIdAsync(int id)
        {
            var imageIds = imagesService.GetImageIdsByTripId(id).ToArray();

            var imageViewModels = await imagesService.GetImagesByIds(imageIds);

            return imageViewModels;
        }
    }
}
