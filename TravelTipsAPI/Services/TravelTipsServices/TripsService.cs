using System;
using System.Security.Claims;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Firebase;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.UtilServices.UtilSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of Trips
    /// </summary>
    /// <param name="context">context</param>
    public class TripsService(
        TravelTipsContext context,
        IUsersService usersService,
        ISpellCheckerService spellCheckerService
    ) : ITripsService
    {
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
        /// Get trips by its title
        /// </summary>
        /// <param name="title">title</param>
        /// <returns>trips contain the title</returns>
        public IEnumerable<TripViewModel> GetTripsByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return [];

            title = title.Trim().ToLower();

            // correct typos in user search input
            var correctedInput = spellCheckerService.CorrectSentence(title);
            Console.WriteLine(title + ": " + correctedInput);

            // find the trips that match the title
            var candidates = context
                .Trips.Where(t => t.IsPublic && t.Title.Contains(correctedInput))
                .ToList();

            // convert to viewModel
            return candidates.Select(trip => new TripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                CreatedBy = (UserViewModel)usersService.GetUserById(trip.CreatedBy),
                CreatedAt = trip.CreatedAt,
                IsPublic = trip.IsPublic,
                NumDays = context.Days.Count(day => day.TripId == trip.Id),
            });
        }

        /// <summary>
        /// Get a trip by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>the trip view model</returns>
        public TripViewModel GetTripByTripId(int id)
        {
            var trip = context.Trips.First(trip => trip.Id == id);

            return new TripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                CreatedBy = (UserViewModel)usersService.GetUserById(trip.CreatedBy),
                CreatedAt = trip.CreatedAt,
                IsPublic = trip.IsPublic,
                NumDays = context.Days.Count(day => day.TripId == trip.Id),
            };
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
                .Select(trip => new TripViewModel
                {
                    Id = trip.Id,
                    Title = trip.Title,
                    Description = trip.Description,
                    CreatedBy = (UserViewModel)usersService.GetUserById(trip.CreatedBy),
                    CreatedAt = trip.CreatedAt,
                    IsPublic = trip.IsPublic,
                    NumDays = context.Days.Where(day => day.TripId == trip.Id).Count(),
                })
                .ToList();

            return yourTripViewModels;
        }

        public Trip? GetTripByDayId(int dayId)
        {
            var day = context.Days.Include(d => d.Trip).FirstOrDefault(d => d.Id == dayId);

            if (day is null)
                return null;

            return day.Trip;
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
        /// <param name="id">trip id</param>
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
        /// Whether you are the owner of the trip
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="tripId">trip id</param>
        /// <returns>true if the owner, false otherwise</returns>
        public bool IsOwner(int id, int tripId)
        {
            var trip = context.Trips.Find(tripId);
            return trip?.CreatedBy == id;
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
