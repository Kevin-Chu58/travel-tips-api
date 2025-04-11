using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.Services
{   
    public class BasicSchema
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
            Task<DayViewModel> PostNewDayAsync(int createdBy, DayPostViewModel newDay);
            Task<DayViewModel> PatchDayAsync(int id, DayPatchViewModel day);
        }

        public interface ILinksService
        {
            IEnumerable<LinkViewModel> GetLinksByName(string name, int createdBy);
            IEnumerable<int> GetYourLinkIds(int id);
            Task<LinkViewModel> PostNewLinkAsync(int createdBy, LinkPostViewModel newLink);
            Task<LinkViewModel> PatchLinkAsync(int id, LinkPatchViewModel link);
        }

        public interface IAttractionsService
        {
            IEnumerable<AttractionViewModel> GetAttractionsByParams(string? name, int? osmId, bool? isPublic, int? ownerId);
            IEnumerable<int> GetYourAttractions(int id);
            Task<AttractionViewModel> PostNewAttractionAsync(int createdBy, AttractionPostViewModel newAttraction);
            Task<AttractionViewModel> PatchAttractionAsync(int id, AttractionPatchViewModel attraction);
        }

        public interface IPreferRoutesService
        {
            // prefer routes
            IEnumerable<PreferRouteViewModel> GetPreferRoutesByParams(int? type, string? reference, int? departOsmId, int? arrivalOsmId, int? estimateTimeMin, int? estimateTimeMax, int? ownerId);
            IEnumerable<int> GetYourPreferRoutes(int id);
            Task<PreferRouteViewModel> PostPreferRoutesAsync(int createdBy, PreferRoutePostViewModel newPreferRoute);
            Task<PreferRouteViewModel> PatchPreferRoutesAsync(int id, PreferRoutePatchViewModel preferRoute);

            // route types
            IEnumerable<RouteTypeViewModel> GetAllRouteTypes();
            Task<RouteTypeViewModel> PostNewRouteTypeAsync(string name);
            Task<RouteTypeViewModel> PatchRouteTypeAsync(int id, string name);
        }
    }

    public class RoleSchema
    {
        public interface IUserRolesService
        {
            bool IsAdmin(int userId);
        }
    }
}
