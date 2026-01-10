using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
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
    /// <param name="usersService">users service</param>
    /// <param name="tripsService">trips service</param>
    /// <param name="tripSharesService">trip shares service</param>
    /// <param name="imagesService">images service</param>
    /// <param name="taosService">trip attraction orders service</param>
    [Route("api/[controller]")]
    public class TripsController(
        IUsersService usersService,
        ITripsService tripsService,
        ITripSharesService tripSharesService,
        ITripAttractionOrdersService taosService,
        IImagesService imagesService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get public trips by title
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
            var tripViewModels = tripsService.GetTripsByParams(
                title: title,
                isPublic: true,
                isHidden: false
            );

            tripViewModels = await AppendImagesToTripsAsync(tripViewModels);

            return Ok(tripViewModels);
        }

        /// <summary>
        /// Get a trip by its id
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
            var trip = tripsService.FindTripByParams(id);

            if (trip is null)
                return NotFound(Messages.TripNotFound);

            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var isShared = tripSharesService.IsTripSharedWithUser(id, userId);

            var isRestricted = trip.CreatedBy == userId || isShared;

            if ((!trip.IsPublic && !isRestricted) || trip.IsHidden)
                return BadRequest(Messages.TripUnauthorized);

            var tripViewModel = tripsService.GetTripViewModel(trip, isRestricted: isRestricted);

            try
            {
                tripViewModel.Images = await GetImagesByTripIdAsync(tripViewModel.Id);
                return Ok(tripViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get my own trips (not hidden)
        /// </summary>
        /// <returns>a list of my own trips</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetMyTrips()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var myTripViewModels = tripsService.GetTripsByParams(
                userId: userId,
                isHidden: false,
                isRestricted: true
            );
            myTripViewModels = await AppendImagesToTripsAsync(myTripViewModels);
            return Ok(myTripViewModels);
        }

        /// <summary>
        /// Get my own trips (hidden)
        /// </summary>
        /// <returns>a list of my own trips</returns>
        [HttpGet]
        [Route("my/hidden")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetMyHiddenTrips()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var myTripViewModels = tripsService.GetTripsByParams(
                userId: userId,
                isPublic: false,
                isHidden: true,
                isRestricted: true
            );
            myTripViewModels = await AppendImagesToTripsAsync(myTripViewModels);
            return Ok(myTripViewModels);
        }

        /// <summary>
        /// Get trips shared with me
        /// </summary>
        /// <returns>a list of trips shared with mes</returns>
        [HttpGet]
        [Route("my/shared")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetSharedTrips()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var sharedTripIds = tripSharesService.GetSharedTripIdsByUserId(userId);

            var myTripViewModels = tripsService.GetTripsByParams(
                ids: sharedTripIds,
                isHidden: false,
                isRestricted: true
            );
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
            // check the trip is either public or the user is the owner or shared user
            var trip = tripsService.FindTripByParams(id);

            if (trip is null)
                return NotFound(Messages.TripNotFound);

            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var isShared = tripSharesService.IsTripSharedWithUser(id, userId);

            var isRestricted = trip.CreatedBy == userId || isShared;

            if ((!trip.IsPublic && !isRestricted) || trip.IsHidden)
                return BadRequest(Messages.TripUnauthorized);

            var geoTripList = taosService.GetTaoGeosByTripId(id, isRestricted);

            return Ok(geoTripList);
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
        /// <param name="id">trip id</param>
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
            var trip = tripsService.FindTripByParams(id);

            if (trip is null)
                return NotFound(Messages.TripNotFound);

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
        public async Task<ActionResult<RegionCompleteViewModel>> UpdateTripRegion(
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
        public async Task<ActionResult<int>> UpdateTripBudget(int id, [FromBody] int? budget)
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

        // trip shares

        /// <summary>
        /// Get a list of shared users by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>a list of shared users</returns>
        [HttpGet]
        [Route("{id}/share")]
        [IsOwner(Resource = Resources.TRIPS)]
        public ActionResult<IEnumerable<UserSimpleViewModel>> GetSharedUsersByTripIdAsync(int id)
        {
            try
            {
                var sharedUserIds = tripSharesService.GetSharedUserIdsByTripId(id);
                var sharedUsers = usersService.GetUsersByIds(sharedUserIds);

                var sharedUserViewModels = sharedUsers.Select(u => (UserSimpleViewModel)u);
                return Ok(sharedUserViewModels);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Share trip with another user
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="userAuthId">shared user auth0 id</param>
        /// <returns>shared user information</returns>
        [HttpPost]
        [Route("{id}/share/{userAuthId}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<UserSimpleViewModel>> ShareTripWithUserAsync(
            int id,
            string userAuthId
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var sharedUser = usersService.GetUserByUserId(userAuthId);

            if (sharedUser is null)
            {
                return NotFound(Messages.UserNotFound);
            }

            if (userId == sharedUser.Id)
            {
                return BadRequest(Messages.TripShareWithSelf);
            }

            try
            {
                await tripSharesService.ShareTripWithUser(id, sharedUser.Id);
                return Ok((UserSimpleViewModel)sharedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unshare trip with another user
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="userAuthId">shared user auth0 id</param>
        /// <returns>unshared user information</returns>
        [HttpDelete]
        [Route("{id}/unshare/{userAuthId}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<UserSimpleViewModel>> UnshareTripWithUserAsync(
            int id,
            string userAuthId
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var unshareUser = usersService.GetUserByUserId(userAuthId);

            if (unshareUser is null)
            {
                return NotFound(Messages.UserNotFound);
            }

            if (userId == unshareUser.Id)
            {
                return BadRequest(Messages.TripUnshareWithSelf);
            }

            try
            {
                await tripSharesService.UnshareTripWithUser(id, unshareUser.Id);
                return Ok((UserSimpleViewModel)unshareUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unshare trip with all shared users
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>the number of shared users removed</returns>
        [HttpDelete]
        [Route("{id}/unshare")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<int>> UnshareTripWithAllAsync(int id)
        {
            try
            {
                var numDeleted = await tripSharesService.UnshareTripWithAll(id);
                return Ok(numDeleted);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // images

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

                if (trip is null)
                    return NotFound(Messages.TripNotFound);

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
