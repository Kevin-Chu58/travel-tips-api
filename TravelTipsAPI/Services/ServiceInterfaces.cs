using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Services
{
    public interface ITripsService
    {
        TripViewModel? GetTripById(int id);
        Task<TripViewModel?> PostNewTripAsync(TripPostViewModel newTrip);
        Task<TripViewModel> UpdateIsPublicAsync(int id, bool isPublic);
        bool IsOwner(int id, int tripId);
    }

    public interface IUsersService
    {
        UserViewModel? GetUserById(int id);
        UserViewModel? GetUserByUserId(string userId);
        Task<UserViewModel?> PostNewUserAsync(string userId);
        Task<UserViewModel?> UpdateUserAsync(UserPatchViewModel newUser);
        bool DoesCurrentUserExist(string userId);
    }
}
