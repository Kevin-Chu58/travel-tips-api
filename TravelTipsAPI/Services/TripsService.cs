
using System;
using System.Security.Claims;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Services
{
    public class TripsService(TravelTipsBasicContext basicContext) : ITripsService
    {
        public TripViewModel? GetTripById(int id)
        {
            var trip = basicContext.Trips.Find(id);

            return (TripViewModel)trip;
        }

        public IEnumerable<TripViewModel> GetTripByName(string name)
        {
            name = name.Trim().ToLower();

            var tripViewModels = new List<TripViewModel>();
            
            if (name.Length >= TripConstants.TRIP_SEARCH_MIN_LENGTH)
            {
               tripViewModels = basicContext.Trips
                .Where(trip => trip.Name.ToLower().Contains(name))
                .Select(trip => (TripViewModel)trip)
                .ToList();
            }

            return tripViewModels;
        }

        public IEnumerable<TripViewModel> GetYourTrips(int id)
        {
            var yourTripViewModels = basicContext.Trips
                .Where(trip => trip.CreatedBy == id)
                .Select(trip => (TripViewModel)trip)
                .ToList();

            return yourTripViewModels;
        }

        public async Task<TripViewModel> PostNewTripAsync(TripPostViewModel newTripViewModel, int createBy)
        {
            var newTrip = newTripViewModel.ToTrip(createBy);

            await basicContext.Trips.AddAsync(newTrip);
            await basicContext.SaveChangesAsync();

            return (TripViewModel)newTrip;
        }

        public async Task<TripViewModel> PatchTripAsync(int id, TripPatchViewModel tripPatchViewModel)
        {
            var trip = basicContext.Trips.Find(id);

            trip.Name = tripPatchViewModel.Name ?? trip.Name;
            trip.Description = tripPatchViewModel.Description ?? trip.Description;

            await basicContext.SaveChangesAsync();

            return (TripViewModel)trip;
        }

        public async Task<TripViewModel> UpdateIsPublicAsync(int id, bool isPublic)
        {
            var trip = basicContext.Trips.Find(id) ?? throw TripsIdNotFoundException(id);
            trip.IsPublic = isPublic;
            
            await basicContext.SaveChangesAsync();
            return (TripViewModel)trip;
        }
        public async Task<TripViewModel> UpdateIsHiddenAsync(int id, bool isHidden)
        {
            var trip = basicContext.Trips.Find(id) ?? throw TripsIdNotFoundException(id);
            trip.IsHidden = isHidden;
            trip.IsPublic = false; // when trashed, also make the trip private

            await basicContext.SaveChangesAsync();
            return (TripViewModel)trip;
        }

        public bool IsOwner(int id, int tripId)
        {
            var trip = basicContext.Trips.Find(tripId);
            return trip?.CreatedBy == id;
        }

        private static Exception TripsIdNotFoundException(int id)
        {
            return new Exception($"Trip not found with id {id}");
        }
    }
}
