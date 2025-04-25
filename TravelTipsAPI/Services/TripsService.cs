using System;
using System.Security.Claims;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Trips
    /// </summary>
    /// <param name="context">context</param>
    public class TripsService(TravelTipsContext context) : ITripsService
    {
        /// <summary>
        /// Return a trip with id from db
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>trip with id</returns>
        public Trip FindTripByParams(int id, bool? isPublic = null)
        {
            var trip = context.Trips.Find(id);

            if (trip is null || (isPublic != null && trip.IsPublic != isPublic))
                throw new Exception(Messages.TripNotFound);

            return trip;
        }

        /// <summary>
        /// Get trips by its name
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>trips contain the name</returns>
        public IEnumerable<TripViewModel> GetTripsByName(string name)
        {
            name = name.Trim().ToLower();

            var tripViewModels = context
                .Trips.Where(trip => trip.Name.ToLower().Contains(name) && trip.IsPublic == true)
                .Select(trip => (TripViewModel)trip)
                .ToList();

            return tripViewModels;
        }

        /// <summary>
        /// Get your trips by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>trips you created</returns>
        public IEnumerable<TripViewModel> GetTripsByUserId(int id)
        {
            var yourTripViewModels = context
                .Trips.Where(trip => trip.CreatedBy == id && trip.IsHidden == false)
                .Select(trip => (TripViewModel)trip)
                .ToList();

            return yourTripViewModels;
        }

        /// <summary>
        /// Get your trips' ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of the ids of trips you own</returns>
        public IEnumerable<int> GetYourTripIds(int id)
        {
            var yourTripIds = context
                .Trips.Where(trip => trip.CreatedBy == id)
                .Select(trip => trip.Id)
                .ToList();

            return yourTripIds;
        }

        /// <summary>
        /// Create a new trip
        /// </summary>
        /// <param name="createBy">the user id created the new trip</param>
        /// <param name="tripPostViewModel">the details of the new trip</param>
        /// <returns>the new trip</returns>
        public async Task<TripViewModel> PostNewTripAsync(
            int createBy,
            TripPostViewModel tripPostViewModel
        )
        {
            var newTrip = tripPostViewModel.ToTrip(createBy);

            await context.Trips.AddAsync(newTrip);
            await context.SaveChangesAsync();

            return (TripViewModel)newTrip;
        }

        /// <summary>
        /// update the trip detail by its id
        /// </summary>
        /// <param name="trip">trip</param>
        /// <param name="tripPatch">trip detail to update</param>
        /// <returns>the updated trip</returns>
        public async Task<TripViewModel> PatchTripAsync(Trip trip, TripPatchViewModel tripPatch)
        {
            trip.Name = tripPatch.Name ?? trip.Name;
            trip.Description = tripPatch.Description ?? trip.Description;
            trip.LastUpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();

            return (TripViewModel)trip;
        }

        /// <summary>
        /// update the trip is public status
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="isPublic">new is public status</param>
        /// <returns>the updated trip</returns>
        public async Task<TripViewModel> UpdateIsPublicAsync(Trip trip, bool isPublic)
        {
            trip.IsHidden = false;
            trip.IsPublic = isPublic;

            await context.SaveChangesAsync();

            return (TripViewModel)trip;
        }

        /// <summary>
        /// Update the trip is hidden status
        /// </summary>
        /// <param name="id">trip id</param>
        /// <param name="isHidden">new is hidden status</param>
        /// <returns>the updated trip</returns>
        public async Task<TripViewModel> UpdateIsHiddenAsync(Trip trip, bool isHidden)
        {
            trip.IsHidden = isHidden;
            trip.IsPublic = false; // when trashed, also make the trip private

            await context.SaveChangesAsync();

            return (TripViewModel)trip;
        }

        /// <summary>
        /// Update the last updated at time
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>the updated trip</returns>
        public async Task<TripViewModel> UpdateLastUpdatedAtAsync(Trip trip)
        {
            trip.LastUpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();

            return (TripViewModel)trip;
        }

        /// <summary>
        /// Whether you are the owner of the trip
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="tripId">trip id</param>
        /// <returns>c</returns>
        public bool IsOwner(int id, int tripId)
        {
            var trip = context.Trips.Find(tripId);
            return trip?.CreatedBy == id;
        }

        /// <summary>
        /// Check if new trip's detail is valid
        /// </summary>
        /// <param name="newTrip">new trip</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePost(TripPostViewModel newTrip)
        {
            var invalidParams = new List<string>();

            if (newTrip.Name.Length > 50)
                invalidParams.Add("name");
            if (newTrip.Description?.Length > 500)
                invalidParams.Add("description");

            return invalidParams;
        }

        public List<string> ValidatePatch(TripPatchViewModel trip)
        {
            var invalidParams = new List<string>();

            if (trip.Name?.Length > 50)
                invalidParams.Add("name");
            if (trip.Description?.Length > 500)
                invalidParams.Add("description");

            return invalidParams;
        }
    }
}
