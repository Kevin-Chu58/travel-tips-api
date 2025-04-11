using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    public class SmallTripsService(TravelTipsContext context, ITripsService tripsService) : ISmallTripsService
    {
        public SmallTripViewModel? GetSmallTripById(int id)
        {
            var smallTrip = context.SmallTrips.Find(id);

            return (SmallTripViewModel)smallTrip;
        }

        public IEnumerable<SmallTripViewModel> GetSmallTripsByTripId(int tripId)
        {
            var smallTripViewModels = context.SmallTrips
                .Where(smallTrip => smallTrip.TripId == tripId)
                .Select(smallTrip => (SmallTripViewModel)smallTrip)
                .ToList();

            return smallTripViewModels;
        }

        public async Task<SmallTripViewModel> PostNewSmallTripsAsync(int tripId, SmallTripPostViewModel smallTripPostViewModel)
        {
            var newSmallTrip = smallTripPostViewModel.ToSmallTrip();
            await context.SmallTrips.AddAsync(newSmallTrip);

            // save changes when update lastUpdatedAt
            await tripsService.UpdateLastUpdatedAtAsync(tripId);

            return (SmallTripViewModel)newSmallTrip;
        }

        public async Task<SmallTripViewModel> PatchSmallTripAsync(int id, TripPatchViewModel smallTripPatchViewModel)
        {
            var smallTrip = context.SmallTrips.Find(id);
            smallTrip.Name = smallTripPatchViewModel.Name ?? smallTrip.Name;
            smallTrip.Description = smallTripPatchViewModel.Description ?? smallTrip.Description;

            // save changes when update lastUpdatedAt
            await tripsService.UpdateLastUpdatedAtAsync(smallTrip.TripId);

            return (SmallTripViewModel)smallTrip;
        }
    }
}
