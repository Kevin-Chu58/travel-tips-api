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
            Trip FindTripByParams(int id, bool? isPublic = null);
            IEnumerable<TripViewModel> GetTripsByName(string name);
            IEnumerable<TripViewModel> GetTripsByUserId(int id);
            IEnumerable<int> GetYourTripIds(int id);
            Task<TripViewModel> PostNewTripAsync(int createdBy, TripPostViewModel newTrip);
            Task<TripViewModel> PatchTripAsync(Trip trip, TripPatchViewModel tripPatch);
            Task<TripViewModel> UpdateIsPublicAsync(Trip trip, bool isPublic);
            Task<TripViewModel> UpdateIsHiddenAsync(Trip trip, bool isHidden);
            Task<TripViewModel> UpdateLastUpdatedAtAsync(Trip trip);
            bool IsOwner(int id, int tripId);
            List<string> ValidatePost(TripPostViewModel newTrip);
        }

        public interface ISmallTripsService
        {
            SmallTripViewModel? GetSmallTripById(int id);
            IEnumerable<SmallTripViewModel> GetSmallTripsByTripId(int tripId);
            Task<SmallTripViewModel> PostNewSmallTripsAsync(
                int tripId,
                SmallTripPostViewModel newSmallTrip
            );
            Task<SmallTripViewModel> PatchSmallTripAsync(int id, TripPatchViewModel smallTrip);
        }

        public interface IDaysService
        {
            Day FindDayById(int id, bool? isPublic = null);
            IEnumerable<DayViewModel> GetDaysByTripId(int tripId);
            IEnumerable<int> GetYourDayIds(int id);
            Task<DayViewModel> PostNewDayAsync(int createdBy, DayPostViewModel newDay);
            Task<DayViewModel> PatchDayAsync(Day day, DayPatchViewModel dayPatch);
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
            IEnumerable<AttractionViewModel> GetAttractionsByParams(
                string? name,
                int? osmId,
                int? ownerId
            );
            IEnumerable<int> GetYourAttractions(int id);
            Task<AttractionViewModel> PostNewAttractionAsync(
                int createdBy,
                AttractionPostViewModel newAttraction
            );
            Task<AttractionViewModel> PatchAttractionAsync(
                int id,
                AttractionPatchViewModel attraction
            );
        }

        public interface IPreferRoutesService
        {
            // prefer routes
            IEnumerable<PreferRouteViewModel> GetPreferRoutesByParams(
                int? type,
                string? reference,
                int? departOsmId,
                int? arrivalOsmId,
                int? estimateTimeMin,
                int? estimateTimeMax,
                int? ownerId
            );
            IEnumerable<int> GetYourPreferRoutes(int id);
            Task<PreferRouteViewModel> PostPreferRoutesAsync(
                int createdBy,
                PreferRoutePostViewModel newPreferRoute
            );
            Task<PreferRouteViewModel> PatchPreferRoutesAsync(
                int id,
                PreferRoutePatchViewModel preferRoute
            );

            // route types
            IEnumerable<RouteTypeViewModel> GetAllRouteTypes();
            Task<RouteTypeViewModel> PostNewRouteTypeAsync(string name);
            Task<RouteTypeViewModel> PatchRouteTypeAsync(int id, string name);
        }

        public interface ITripAttractionOrdersService
        {
            // taos
            TripAttractionOrderViewModel GetTripAttractionOrderById(int id, bool? isPublic);
            IEnumerable<int> GetYourTripAttractionOrders(int id);
            Task<TripAttractionOrderViewModel> PostTripAttractionOrderAsync(
                int createdBy,
                TripAttractionOrderPostViewModel newTripAttractionOrder
            );
            Task<TripAttractionOrderViewModel> PatchTripAttractionOrderAsync(
                int id,
                TripAttractionOrderPatchViewModel tripAttractionOrder
            );
            Task<IEnumerable<TripAttractionOrderViewModel>> SetOrderAsync(int id, int newOrder);
            Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderAsync(int id);

            // taors
            Task<TripAttractionOrderViewModel> PostNewTripAttractionOrderRouteAsync(
                int id,
                int preferRouteId,
                int order
            );
            Task<TripAttractionOrderViewModel> SetPreferRouteOrderAsync(
                int id,
                int preferRouteId,
                int newOrder
            );
            Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderRouteAsync(
                int id,
                int preferRouteId
            );
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
