
using System.Security.Claims;
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

        public async Task<TripViewModel> PostNewTripAsync(TripPostViewModel newTripViewModel, int createBy)
        {
            var newTrip = newTripViewModel.ToTrip(createBy);

            await basicContext.Trips.AddAsync(newTrip);
            await basicContext.SaveChangesAsync();

            return (TripViewModel)newTrip;
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
