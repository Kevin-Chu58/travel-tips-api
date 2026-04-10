using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Constants.OrderBy.TripOrderBy;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;
using static TravelTipsAPI.Utils.ObjectUtils;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

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
        TravelTipsContext context,
        IRegionsService regionsService,
        IBookmarksService bookmarksService,
        IUsersService usersService,
        IUserExtendsService userExtendsService,
        ITripsService tripsService,
        ITripSharesService tripSharesService,
        ITripAttractionOrdersService taosService,
        IImagesService imagesService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get public trips by params
        /// </summary>
        /// <param name="title">the title of the trips</param>
        /// <param name="createdByAuthId">the creator user id</param>
        /// <param name="countrySlug">the country slug</param>
        /// <param name="stateSlug">the state slug</param>
        /// <param name="budget">the budget</param>
        /// <param name="cursor">the pagination cursor</param>
        /// <param name="tripOrderByEnum">order by enum</param>
        /// <param name="limit">number limit</param>
        /// <returns>research result of trips that fit the params with cursor optionally</returns>
        [HttpGet]
        [Route("")]
        //[AllowAnonymous]
        [IsOwner(Resource = Resources.NONE)] // requires login as personal project
        public async Task<ActionResult<SearchResults<TripViewModel>>> GetTripsByParams(
            [FromQuery] string? title,
            string? createdByAuthId = null,
            string? countrySlug = null,
            string? stateSlug = null,
            int? budget = null,
            string? cursor = null,
            TripOrderByEnum? tripOrderByEnum = null,
            int? limit = null
        )
        {
            limit ??= Global.TRIP_DEFAULT_LIMIT;
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // get createdBy user id (not user's userId)
            int? createdBy = null;
            if (!string.IsNullOrEmpty(createdByAuthId))
            {
                var createdByUser = usersService.GetUserByUserId(createdByAuthId);
                if (createdByUser is null)
                    return NotFound(Messages.UserNotFound);
                createdBy = createdByUser.Id;
            }

            // get region
            RegionViewModel? region = null;
            try
            {
                if (!string.IsNullOrEmpty(countrySlug))
                {
                    region = regionsService.GetRegionByCountryAndState(countrySlug, stateSlug);
                }
                else if (!string.IsNullOrEmpty(stateSlug))
                    return NotFound(Messages.RegionInvalid);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

            // set default order by id desc if no order provided
            if (tripOrderByEnum is null)
                tripOrderByEnum = TripOrderByEnum.Newest;

            // decode cursor if provided
            TripCursor? tripCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                tripCursor = DecodeCursor<TripCursor>(cursor);
                if (tripCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            var tripViewModels = await tripsService.GetTripsByParams(
                userId: userId,
                title: title,
                isPublic: true,
                isHidden: false,
                createdBy: createdBy,
                region: region,
                budget: budget,
                cursor: tripCursor,
                tripOrderByEnum: tripOrderByEnum,
                limit: limit
            );

            tripViewModels = await AppendImagesToTripsAsync(tripViewModels);

            // encode cursor
            var tripList = tripViewModels.ToList();
            string? newCursor = null;
            if (tripList.Count == limit)
            {
                var lastTrip = tripList.Last();
                newCursor = EncodeCursor(
                    new TripCursor
                    {
                        Id = lastTrip.Id,
                        CreatedAt = lastTrip.CreatedAt,
                        BookmarkCount = lastTrip.BookmarkCount,
                    }
                );
            }

            var result = new SearchResults<TripViewModel>
            {
                Results = tripList,
                Cursor = newCursor,
            };

            return Ok(result);
        }

        /// <summary>
        /// Get a trip by its id
        /// </summary>
        /// <param name="id">the id of a trip</param>
        /// <returns>a trip with that id, Not Found otherwise</returns>
        [HttpGet]
        [Route("{id}")]
        //[AllowAnonymous]
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

            var tripViewModel = await tripsService.GetTripViewModel(
                trip,
                userId,
                isRestricted: isRestricted
            );

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

            var myTripViewModels = await tripsService.GetTripsByParams(
                userId,
                createdBy: userId,
                isHidden: false,
                isRestricted: true,
                isMy: true
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

            var myTripViewModels = await tripsService.GetTripsByParams(
                userId,
                createdBy: userId,
                isPublic: false,
                isHidden: true,
                isRestricted: true,
                isMy: true
            );
            myTripViewModels = await AppendImagesToTripsAsync(myTripViewModels);
            return Ok(myTripViewModels);
        }

        /// <summary>
        /// Get trips shared with me
        /// </summary>
        /// <returns>a list of trips shared with me</returns>
        [HttpGet]
        [Route("my/shared")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetSharedTrips()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var sharedTripIds = tripSharesService.GetSharedTripIdsByUserId(userId);

            var myTripViewModels = await tripsService.GetTripsByParams(
                userId,
                ids: sharedTripIds,
                isHidden: false,
                isRestricted: true
            );
            myTripViewModels = await AppendImagesToTripsAsync(myTripViewModels);
            return Ok(myTripViewModels);
        }

        /// <summary>
        /// Get my bookmarked trips
        /// </summary>
        /// <returns>a list of trips I bookmarked</returns>
        [HttpGet]
        [Route("my/bookmarked")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<IEnumerable<TripViewModel>>> GetBookMarkedTrips()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var bookmarkedTripIds = bookmarksService.GetBookmarkTripIdsByUserId(userId);

            var myTripViewModels = await tripsService.GetTripsByParams(
                userId,
                ids: bookmarkedTripIds,
                isPublic: true,
                isHidden: false
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
        /// <param name="newTrip">new trip</param>
        /// <returns>the new trip posted to db</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<TripViewModel>> PostNewTrip(
            [FromBody] TripPostViewModel newTrip
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                var userSubExtend = await userExtendsService.GetUpdatedUserSubExtendByUserId(
                    userId
                );

                if (userSubExtend.TripCount >= userSubExtend.MaxTripCount)
                    return BadRequest(Messages.MembershipRequired);

                var tx = await context.Database.BeginTransactionAsync();

                // create the new trip if max trip count is not reached,
                // and update the user's trip count in user sub extend
                var tripViewModel = await tripsService.PostNewTripAsync(userId, newTrip.Title);
                tripViewModel.IsReadonly = false;

                await userExtendsService.UpdateSubExtendTripCount(userSubExtend, 1);

                await tx.CommitAsync();

                return Ok(tripViewModel);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var trip = tripsService.FindTripByParams(id);

            if (trip is null)
                return NotFound(Messages.TripNotFound);

            var isEditable = tripsService.CanUserEditTrip(trip.Id, userId);
            if (!isEditable)
                return BadRequest(Messages.MembershipRequired);

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
                return BadRequest(Messages.TripUnauthorized);

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
                return BadRequest(Messages.TripUnauthorized);

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
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                var trip = tripsService.FindTripByParams(id);
                if (trip is null)
                    return NotFound(Messages.TripNotFound);

                var isEditable = tripsService.CanUserEditTrip(trip.Id, userId);
                if (!isEditable)
                    return BadRequest(Messages.MembershipRequired);

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
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            try
            {
                var trip = tripsService.FindTripByParams(id);
                if (trip is null)
                    return NotFound(Messages.TripNotFound);

                var isEditable = tripsService.CanUserEditTrip(trip.Id, userId);
                if (!isEditable)
                    return BadRequest(Messages.MembershipRequired);

                var newBudget = await tripsService.UpdateBudgetAsync(trip, budget);

                return Ok(newBudget);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIPS)]
        public async Task<ActionResult<int>> DeleteTripAsync(int id)
        {
            var trip = tripsService.FindTripByParams(id);
            if (trip is null)
                return NotFound(Messages.TripNotFound);

            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);
                var userSubExtend = await userExtendsService.GetUpdatedUserSubExtendByUserId(
                    userId
                );

                var tx = await context.Database.BeginTransactionAsync();

                // delete the trip, and update the user's trip count in user sub extend
                var tripId = await tripsService.DeleteTripAsync(trip);
                await userExtendsService.UpdateSubExtendTripCount(userSubExtend, -1);

                await tx.CommitAsync();
                return Ok(tripId);
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
        public async Task<
            ActionResult<IEnumerable<UserSimpleViewModel>>
        > GetSharedUsersByTripIdAsync(int id)
        {
            try
            {
                var sharedUserIds = tripSharesService.GetSharedUserIdsByTripId(id);
                var sharedUsers = usersService.GetUsersByIds(sharedUserIds);

                var sharedUserViewModels = await usersService.GetUserSimpleViewModels(sharedUsers);
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
        [HasRole(Role = UserRoles.MEMBER)]
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
                var sharedUserViewModel = (
                    await usersService.GetUserSimpleViewModels([sharedUser])
                ).First();
                return Ok(sharedUserViewModel);
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
        [HasRole(Role = UserRoles.MEMBER)]
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
        [HasRole(Role = UserRoles.MEMBER)]
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

        // bookmarks

        [HttpPost]
        [Route("{id}/bookmark")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult> AddBookmark(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            try
            {
                await tripsService.BookmarkAsync(userId, id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id}/bookmark")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult> RemoveBookmark(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            try
            {
                await tripsService.UnbookmarkAsync(userId, id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
