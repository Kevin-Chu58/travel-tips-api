using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels.db_basic;

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
        TripViewModel? GetTripById(int id, bool? isPublic = null);
        IEnumerable<TripViewModel> GetTripsByName(string name);
        IEnumerable<TripViewModel> GetTripsByUserId(int id);
        IEnumerable<int> GetYourTripIds(int id);
        Task<TripViewModel> PostNewTripAsync(int createdBy, TripPostViewModel newTrip);
        Task<TripViewModel> PatchTripAsync(int id, TripPatchViewModel trip);
        Task<TripViewModel> UpdateIsPublicAsync(int id, bool isPublic);
        Task<TripViewModel> UpdateIsHiddenAsync(int id, bool isHidden);
        Task<TripViewModel> UpdateLastUpdatedAtAsync(int id);
        bool IsOwner(int id, int tripId);
    }

    public interface ISmallTripsService
    {
        SmallTripViewModel? GetSmallTripById(int id);
        IEnumerable<SmallTripViewModel> GetSmallTripsByTripId(int tripId);
        Task<SmallTripViewModel> PostNewSmallTripsAsync(int tripId, SmallTripPostViewModel newSmallTrip);
        Task<SmallTripViewModel> PatchSmallTripAsync(int id, TripPatchViewModel smallTrip);
    }

    public interface IDaysService
    {
        DayViewModel? GetDayById(int id);
        IEnumerable<DayViewModel> GetDaysByTripId(int tripId);
        IEnumerable<int> GetYourDayIds(int id);
        Task<DayViewModel> PostNewDayAsync(int tripId, int createdBy, DayPostViewModel newDay);
        Task<DayViewModel> PatchDayAsync(int id, DayPatchViewModel day);
    }

    public interface ILinksService
    {
        LinkSearchViewModel GetLinksByName(int timeStamp, string name, int createdBy);
        Task<LinkViewModel> PostNewLinkAsync(int createdBy, LinkPostViewModel newLink);
        Task<LinkViewModel> PatchLinkAsync(int id, LinkPatchViewModel link);
        bool IsOwner(int id, int linkId);
    }
}
