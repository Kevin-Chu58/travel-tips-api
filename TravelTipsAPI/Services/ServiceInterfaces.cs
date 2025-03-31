using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Services
{   
    public interface IUsersService
    {
        UserViewModel? GetUserById(int id);
        Task<UserViewModel> GetUserByUserId(string userId);
        Task<UserViewModel> PostNewUserAsync(string userId);
        Task<UserViewModel> UpdateUserAsync(int id, UserPatchViewModel newUser);
    }
    public interface ITripsService
    {
        TripViewModel? GetTripById(int id);
        IEnumerable<TripViewModel> GetTripByName(string name);
        IEnumerable<TripViewModel> GetYourTrips(int id);
        Task<TripViewModel> PostNewTripAsync(TripPostViewModel newTrip, int createdBy);
        Task<TripViewModel> PatchTripAsync(int id, TripPatchViewModel trip);
        Task<TripViewModel> UpdateIsPublicAsync(int id, bool isPublic);
        Task<TripViewModel> UpdateIsHiddenAsync(int id, bool isHidden);
        bool IsOwner(int id, int tripId);
    }
}
