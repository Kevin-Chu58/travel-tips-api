using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of Trips
    /// </summary>
    /// <param name="context">context</param>
    public class TripsService(
        TravelTipsContext context,
        IUsersService usersService,
        IRegionsService regionsService,
        ITripSharesService tripSharesService
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
        /// <param name="isRestricted">is user owner or shared with</param>
        /// <returns>the trip view model</returns>
        public TripViewModel? GetTripById(int id, bool isRestricted = false)
        {
            var trip = GetTripsByParams(ids: [id], isRestricted: isRestricted).FirstOrDefault();
            return trip;
        }

        /// <summary>
        /// Get trip view model from trip
        /// </summary>
        /// <param name="trip">trip</param>
        /// <param name="isRestricted">is user owner or shared with</param>
        /// <returns>trip view model of that trip</returns>
        public TripViewModel GetTripViewModel(Trip trip, bool isRestricted = false)
        {
            var tripViewModel = new TripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                CreatedBy = (UserViewModel)usersService.GetUserById(trip.CreatedBy),
                CreatedAt = trip.CreatedAt,
                IsPublic = trip.IsPublic,
                IsHidden = trip.IsHidden,
                NumDays = context.Days.Count(day => day.TripId == trip.Id),
                Region =
                    trip.RegionId != null
                        ? regionsService.BuildRegionComplete(trip.RegionId.Value)
                        : null,
                Budget = trip.Budget,
                SharedUsers = isRestricted
                    ? tripSharesService
                        .GetSharedUserIdsByTripId(trip.Id)
                        .Select(userId => (UserSimpleViewModel)usersService.GetUserById(userId))
                        .ToList()
                    : [],
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
        /// <param name="id">trip id</param>
        /// <param name="title">title</param>
        /// <param name="userId">user id</param>
        /// <param name="isPublic">is public</param>
        /// <param name="isHidden">is hidden</param>
        /// <param name="regionId">region id</param>
        /// <param name="budget">budget</param>
        /// <param name="isRestricted">is user owner or shared with</param>
        /// <returns>a list of trips</returns>
        public IEnumerable<TripViewModel> GetTripsByParams(
            IEnumerable<int>? ids = null,
            string? title = null,
            int? userId = null,
            bool? isPublic = null,
            bool? isHidden = null,
            int? regionId = null,
            int? budget = null,
            bool isRestricted = false
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
            if (userId != null)
            {
                query = query.Where(t => t.CreatedBy == userId);
            }
            if (isPublic != null)
            {
                query = query.Where(t => t.IsPublic == isPublic);
            }
            if (isHidden != null)
            {
                query = query.Where(t => t.IsHidden == isHidden);
            }
            if (regionId != null)
            {
                query = query.Where(t => t.RegionId == regionId);
            }
            if (budget != null)
            {
                query = query.Where(t => t.Budget == budget);
            }

            var trips = query.ToList();

            return trips.Select(trip => GetTripViewModel(trip, isRestricted));
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

            var newTripViewModel = (TripViewModel)newTrip;
            newTripViewModel.CreatedBy = (UserViewModel)usersService.GetUserById(createdBy);
            newTripViewModel.Region =
                newTrip.RegionId != null
                    ? regionsService.BuildRegionComplete(newTrip.RegionId.Value)
                    : null;

            return newTripViewModel;
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
            trip.Title = tripPatch.Title?.Trim() ?? trip.Title;
            trip.Description = tripPatch.Description?.Trim() ?? trip.Description;

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

        /// <summary>
        /// Check if new trip's detail is valid
        /// </summary>
        /// <param name="name">new trip name</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePost(string name)
        {
            var invalidParams = new List<string>();

            if (name.Length > 50)
                invalidParams.Add("name");

            return invalidParams;
        }

        /// <summary>
        /// Check if trip's detail is valid
        /// </summary>
        /// <param name="trip">existing trip</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePatch(TripPatchViewModel trip)
        {
            var invalidParams = new List<string>();

            if (trip.Title?.Length == 0 || trip.Title?.Length > 50)
                invalidParams.Add("name");

            return invalidParams;
        }
    }
}
