using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Services
{   
    public interface IUsersService
    {
        UserViewModel? GetUserById(int id);
        Task<UserViewModel> GetUserByUserId(string userId);
        Task<UserViewModel> PostNewUserAsync(string userId);
        Task<UserViewModel> UpdateUserAsync(UserPatchViewModel newUser);
    }
    public interface ITripsService
    {
        TripViewModel? GetTripById(int id);
        Task<TripViewModel> PostNewTripAsync(TripPostViewModel newTrip, int createdBy);
        Task<TripViewModel> UpdateIsPublicAsync(int id, bool isPublic);
        Task<TripViewModel> UpdateIsHiddenAsync(int id, bool isHidden);
        bool IsOwner(int id, int tripId);
    }
}
