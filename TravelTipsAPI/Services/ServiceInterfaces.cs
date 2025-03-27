using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Services
{
    public interface ITripsService
    {
        TripViewModel? GetTripById(int id);
        Task<TripViewModel?> PostNewTripAsync(TripPostViewModel newTrip);
        Task<TripViewModel> UpdateIsPublicAsync(int id, bool isPublic);
    }
}
