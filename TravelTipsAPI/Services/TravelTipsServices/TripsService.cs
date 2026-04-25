using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Constants.OrderBy.TripOrderBy;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of Trips
    /// </summary>
    /// <param name="context">context</param>
    public class TripsService(
        TravelTipsContext context,
        IBookmarksService bookmarksService,
        IUsersService usersService,
        IRegionsService regionsService,
        ITripSharesService tripSharesService,
        ILogger<TripsService> logger
    ) : ITripsService
    {
        /// <summary>
        /// Return a trip with id from db
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="dayId">day id</param>
        /// <param name="isPublic">is trip public</param>
        /// <returns>trip with id</returns>
        public Trip? FindTripByParams(int? id = null, int? dayId = null, bool? isPublic = null)
        {
            IQueryable<Trip> query = context.Trips;

            if (id.HasValue)
            {
                query = query.Where(t => t.Id == id.Value);
            }

            if (dayId.HasValue)
            {
                query = query.Where(t => t.Days.Any(d => d.Id == dayId.Value));
            }

            if (isPublic.HasValue)
            {
                query = query.Where(t => t.IsPublic == isPublic.Value);
            }

            return query.FirstOrDefault();
        }

        /// <summary>
        /// Get a trip by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="userId">my user id if any</param>
        /// <param name="isRestricted">is user owner or shared with</param>
        /// <param name="isMy">whether only get my trips, used for my trip page</param>
        /// <returns>the trip view model</returns>
        public async Task<TripViewModel?> GetTripById(
            int id,
            int? userId = null,
            bool isRestricted = false,
            bool isMy = false
        )
        {
            var trip = (
                await GetTripsByParams(
                    ids: [id],
                    userId: userId,
                    isRestricted: isRestricted,
                    isMy: isMy
                )
            ).FirstOrDefault();
            return trip;
        }

        /// <summary>
        /// Get trip view model from trip
        /// </summary>
        /// <param name="trip">trip</param>
        /// <param name="userId">my user id if any</param>
        /// <param name="users">users</param>
        /// <param name="dayCounts">day counts</param>
        /// <param name="isRestricted">is user owner or shared with</param>
        /// <returns>trip view model of that trip</returns>
        public async Task<TripViewModel> GetTripViewModel(
            Trip trip,
            int? userId = null,
            IEnumerable<UserSimpleViewModel>? users = null,
            Dictionary<int, int>? dayCounts = null,
            bool isRestricted = false,
            IEnumerable<int>? editableTripIds = null
        )
        {
            // get simple user
            UserSimpleViewModel? simpleUser = null;
            if (users?.Any() == true)
                simpleUser = users.FirstOrDefault(user => user.Id == trip.CreatedBy);

            if (simpleUser is null)
            {
                var user = usersService.GetUserById(trip.CreatedBy);
                simpleUser = (await usersService.GetUserSimpleViewModels([user])).First();
            }

            // get day count
            int? dayCount = null;
            if (dayCounts?.Count > 0)
            {
                dayCount = dayCounts?.GetValueOrDefault(trip.Id, 0);
            }

            // only get shared users when get trip by id
            IEnumerable<UserSimpleViewModel> sharedSimpleUsers = [];
            if (isRestricted)
            {
                var sharedUsers = tripSharesService.GetSharedUserIdsByTripId(trip.Id).ToList();
                sharedSimpleUsers = await usersService.GetUserSimpleViewModels(
                    usersService.GetUsersByIds(sharedUsers)
                );
            }

            var tripViewModel = new TripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                CreatedBy = simpleUser,
                CreatedAt = trip.CreatedAt,
                IsPublic = trip.IsPublic,
                IsHidden = trip.IsHidden,
                IsBookmarked = context.Bookmarks.Any(b =>
                    b.UserId == userId && b.TripId == trip.Id
                ),
                NumDays = dayCount ?? context.Days.Count(day => day.TripId == trip.Id),
                BookmarkCount = trip.BookmarkCount,
                Region =
                    trip.RegionId != null
                        ? regionsService.BuildRegionComplete(trip.RegionId.Value)
                        : null,
                Budget = trip.Budget,
                SharedUsers = sharedSimpleUsers,
                IsReadonly = editableTripIds == null || !editableTripIds.Contains(trip.Id),
            };
            return tripViewModel;
        }

        /// <summary>
        /// Get my trips' ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of the ids of trips I own</returns>
        public IEnumerable<int> GetMyTripIds(int id)
        {
            var myTripIds = context
                .Trips.Where(trip => trip.CreatedBy == id)
                .Select(trip => trip.Id)
                .ToList();

            return myTripIds;
        }

        /// <summary>
        /// Get trips by params
        /// </summary>
        /// <param name="userId">my user id if any</param>
        /// <param name="ids">trip ids</param>
        /// <param name="title">title</param>
        /// <param name="createdBy">createdBy</param>
        /// <param name="isPublic">is public</param>
        /// <param name="isHidden">is hidden</param>
        /// <param name="region">region</param>
        /// <param name="budget">budget</param>
        /// <param name="isRestricted">is user owner or shared with</param>
        /// <param name="cursor">trip cursor</param>
        /// <param name="tripOrderByEnum">order by enum</param>
        /// <param name="limit">limit</param>
        /// <param name="isMy">whether only get my trips, used for my trip page</param>
        /// <returns>a list of trips</returns>
        public async Task<IEnumerable<TripViewModel>> GetTripsByParams(
            int? userId = null,
            IEnumerable<int>? ids = null,
            string? title = null,
            int? createdBy = null,
            bool? isPublic = null,
            bool? isHidden = null,
            RegionViewModel? region = null,
            int? budget = null,
            bool isRestricted = false,
            TripCursor? cursor = null,
            TripOrderByEnum? tripOrderByEnum = null,
            int? limit = null,
            bool isMy = false
        )
        {
            var query = context.Trips.AsQueryable();

            if (ids != null)
            {
                query = query.Where(t => ids.Contains(t.Id));
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim().ToLower();
                query = query.Where(t => t.Title.Contains(title));
            }
            if (createdBy != null)
            {
                query = query.Where(t => t.CreatedBy == createdBy);
            }
            if (isPublic != null)
            {
                query = query.Where(t => t.IsPublic == isPublic);
            }
            if (isHidden != null)
            {
                query = query.Where(t => t.IsHidden == isHidden);
            }
            if (region != null)
            {
                if (region.Type == "State")
                    query = query.Where(t => t.RegionId == region.Id);
                // if region is country, then also include all its states
                else if (region.Type == "Country")
                {
                    //var stateIds = context
                    //    .Regions.Where(r => r.ParentRegionId == region.Id || r.Id == region.Id)
                    //    .Select(r => r.Id);
                    query = query.Where(t =>
                        t.RegionId != null
                        && context
                            .Regions.Where(r => r.ParentRegionId == region.Id || r.Id == region.Id)
                            .Select(r => r.Id)
                            .Contains(t.RegionId.Value)
                    );
                }
            }
            if (budget != null)
            {
                query = query.Where(t => t.Budget == budget);
            }

            if (tripOrderByEnum != null)
            {
                query = ApplyCursor(query, cursor, tripOrderByEnum);
            }

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }
            var trips = query.ToList();

            // user preload
            var distinctUserIds = trips.Select(t => t.CreatedBy).Distinct();
            var users = usersService.GetUsersByIds(distinctUserIds);
            var simpleUsers = await usersService.GetUserSimpleViewModels(users);

            // subscription preload
            var editableTripIds =
                isMy == true && userId != null ? GetEditableTripIds(userId.Value) : [];

            // days preload
            var tripIds = trips.Select(t => t.Id).ToList();
            var dayCounts = context
                .Days.Where(d => tripIds.Contains(d.TripId))
                .GroupBy(d => d.TripId)
                .Select(g => new { TripId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.TripId, x => x.Count);

            var results = new List<TripViewModel>();

            foreach (var trip in trips)
            {
                results.Add(
                    await GetTripViewModel(
                        trip,
                        userId,
                        simpleUsers,
                        dayCounts,
                        isRestricted,
                        editableTripIds
                    )
                );
            }

            return results;
        }

        /// <summary>
        /// Apply cursor to the query
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="cursor">trip cursor</param>
        /// <param name="tripOrderByEnum">order by</param>
        /// <returns>the query with applied cursor</returns>
        private static IQueryable<Trip> ApplyCursor(
            IQueryable<Trip> query,
            TripCursor? cursor,
            TripOrderByEnum? tripOrderByEnum
        )
        {
            switch (tripOrderByEnum)
            {
                case TripOrderByEnum.Newest:
                    query = query.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id);
                    if (cursor != null)
                        query = query.Where(t =>
                            t.CreatedAt < cursor.CreatedAt
                            || (t.CreatedAt == cursor.CreatedAt && t.Id < cursor.Id)
                        );
                    break;

                case TripOrderByEnum.Oldest:
                    query = query.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id);
                    if (cursor != null)
                        query = query.Where(t =>
                            t.CreatedAt > cursor.CreatedAt
                            || (t.CreatedAt == cursor.CreatedAt && t.Id > cursor.Id)
                        );
                    break;

                case TripOrderByEnum.MostBookmarked:
                    query = query
                        .OrderByDescending(t => t.BookmarkCount)
                        .ThenByDescending(t => t.Id);
                    if (cursor != null)
                        query = query.Where(t =>
                            t.BookmarkCount < cursor.BookmarkCount
                            || (t.BookmarkCount == cursor.BookmarkCount && t.Id < cursor.Id)
                        );
                    break;

                case TripOrderByEnum.LeastBookmarked:
                    query = query.OrderBy(t => t.BookmarkCount).ThenBy(t => t.Id);
                    if (cursor != null)
                        query = query.Where(t =>
                            t.BookmarkCount > cursor.BookmarkCount
                            || (t.BookmarkCount == cursor.BookmarkCount && t.Id > cursor.Id)
                        );
                    break;
            }

            return query;
        }

        /// <summary>
        /// Create a new trip
        /// </summary>
        /// <param name="createdBy">the user id created the new trip</param>
        /// <param name="title">the new trip title</param>
        /// <returns>the new trip</returns>
        public async Task<TripViewModel> PostNewTripAsync(int createdBy, string title)
        {
            var newTrip = new Trip
            {
                Title = title,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
            };

            await context.Trips.AddAsync(newTrip);
            await context.SaveChangesAsync();

            var newTripViewModel = await GetTripById(newTrip.Id, isMy: true)!;

            return newTripViewModel!;
        }

        /// <summary>
        /// update the trip detail by its id
        /// </summary>
        /// <param name="trip">trip</param>
        /// <param name="tripPatch">trip detail to update</param>
        /// <returns>the updated trip</returns>
        public async Task<TripPatchViewModel> PatchTripAsync(
            Trip trip,
            TripPatchViewModel tripPatch
        )
        {
            // Title: override only if non-null (validation already guarantees min length)
            if (tripPatch.Title is not null)
            {
                trip.Title = tripPatch.Title.Trim();
            }

            // Description: null = no change, empty = clear
            if (tripPatch.Description is not null)
            {
                trip.Description = tripPatch.Description.Trim();
            }

            await context.SaveChangesAsync();

            return tripPatch;
        }

        /// <summary>
        /// update the trip is public status
        /// </summary>
        /// <param name="tripIds">trip ids</param>
        /// <param name="isPublic">new is public status</param>
        /// <returns>the updated trip</returns>
        public async Task<List<int>> UpdateIsPublicAsync(int[] tripIds, bool isPublic)
        {
            var _tripIds = new List<int>();
            foreach (var tripId in tripIds)
            {
                var trip = context.Trips.Find(tripId);
                trip!.IsHidden = false;
                trip.IsPublic = isPublic;

                _tripIds.Add(tripId);
            }

            await context.SaveChangesAsync();

            return _tripIds;
        }

        /// <summary>
        /// Update the trip is hidden status
        /// </summary>
        /// <param name="tripIds">trip ids</param>
        /// <param name="isHidden">new is hidden status</param>
        /// <returns>the updated trip</returns>
        public async Task<List<int>> UpdateIsHiddenAsync(int[] tripIds, bool isHidden)
        {
            var _tripIds = new List<int>();
            foreach (var tripId in tripIds)
            {
                var trip = context.Trips.Find(tripId);
                trip!.IsHidden = isHidden;
                trip.IsPublic = false; // when trashed, also make the trip private

                _tripIds = [.. _tripIds, tripId];
            }

            await context.SaveChangesAsync();

            return _tripIds;
        }

        /// <summary>
        /// Update the trip region
        /// </summary>
        /// <param name="trip">the trip to be updated</param>
        /// <param name="regionId">region id</param>
        /// <returns>the updated complete region</returns>
        public async Task<RegionCompleteViewModel> UpdateRegionAsync(Trip trip, int? regionId)
        {
            trip.RegionId = regionId;

            await context.SaveChangesAsync();

            return regionId != null
                ? regionsService.BuildRegionComplete((int)regionId)
                : new RegionCompleteViewModel();
        }

        /// <summary>
        /// Update the trip budget
        /// </summary>
        /// <param name="trip">the trip to be updated</param>
        /// <param name="budget">budget</param>
        /// <returns>the updated budget</returns>
        public async Task<int> UpdateBudgetAsync(Trip trip, int? budget)
        {
            if (budget < 1 || budget > 5)
                throw new Exception(Messages.TripBudgetInvalid);

            trip.Budget = budget;
            await context.SaveChangesAsync();
            return trip.Budget ?? 0;
        }

        public async Task<int> DeleteTripAsync(Trip trip)
        {
            context.Trips.Remove(trip);
            await context.SaveChangesAsync();

            return trip.Id;
        }

        /// <summary>
        /// Whether you are the owner of a list of trips
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="tripIds">trip ids</param>
        /// <returns>true if the owner of all, false otherwise</returns>
        public bool IsOwnerList(int id, int[] tripIds)
        {
            var myTripIds = GetMyTripIds(id);
            return tripIds.All(tripId => myTripIds.Contains(tripId));
        }

        // bookmarks

        public async Task BookmarkAsync(int userId, int tripId)
        {
            var tx = await context.Database.BeginTransactionAsync();

            await bookmarksService.AddBookmarkAsync(userId, tripId);
            await UpdateBookmarkCountAsync(tripId, true);

            await tx.CommitAsync();
        }

        public async Task UnbookmarkAsync(int userId, int tripId)
        {
            var tx = await context.Database.BeginTransactionAsync();

            await bookmarksService.RemoveBookmarkAsync(userId, tripId);
            await UpdateBookmarkCountAsync(tripId, false);

            await tx.CommitAsync();
        }

        /// <summary>
        /// Update the bookmark count on trip
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <param name="increment">whether is increase or decrease</param>
        /// <returns></returns>
        private async Task UpdateBookmarkCountAsync(int tripId, bool increment)
        {
            if (increment)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE db_basic.Trips SET BookmarkCount = BookmarkCount + 1 WHERE Id = @id",
                    new SqlParameter("@id", tripId)
                );
            }
            else
            {
                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE db_basic.Trips SET BookmarkCount = BookmarkCount - 1 WHERE Id = @id",
                    new SqlParameter("@id", tripId)
                );
            }
        }

        // subscriptions

        /// <summary>
        /// Get a list of trip ids that the user can edit based on the subscription
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of trip ids</returns>
        public IEnumerable<int> GetEditableTripIds(int userId)
        {
            var maxTripCount = context
                .UserSubExtends.Where(use => use.UserId == userId)
                .Select(use => use.MaxTripCount)
                .FirstOrDefault();

            var editableTripIds = context
                .Trips.Where(t => t.CreatedBy == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Take(maxTripCount)
                .Select(t => t.Id)
                .ToList();

            logger.LogInformation(editableTripIds.ToArray().ToString());

            return editableTripIds;
        }

        /// <summary>
        /// Check if a user can modify a trip based on max trip count in their subscription
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <param name="userId">user id</param>
        /// <returns>whether user can modify the trip</returns>
        public bool CanUserEditTrip(int tripId, int userId)
        {
            var maxTripCount = context
                .UserSubExtends.Where(use => use.UserId == userId)
                .Select(use => use.MaxTripCount)
                .FirstOrDefault();

            // only check the most recent maxTripCount trips due to subsctipion status
            return context
                .Trips.Where(t => t.CreatedBy == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Take(maxTripCount)
                .Any(t => t.Id == tripId);
        }
    }
}
