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
            IEnumerable<int> GetMyTripIds(int id);
            Task<TripViewModel> PostNewTripAsync(int createdBy, TripPostViewModel newTrip);
            Task<TripViewModel> PatchTripAsync(Trip trip, TripPatchViewModel tripPatch);
            Task<TripViewModel> UpdateIsPublicAsync(Trip trip, bool isPublic);
            Task<TripViewModel> UpdateIsHiddenAsync(Trip trip, bool isHidden);
            Task<TripViewModel> UpdateLastUpdatedAtAsync(Trip trip);
            bool IsOwner(int id, int tripId);
            List<string> ValidatePost(TripPostViewModel newTrip);
            List<string> ValidatePatch(TripPatchViewModel trip);
        }

        public interface IDaysService
        {
            Day FindDayById(int id, bool? isPublic = null);
            IEnumerable<DayViewModel> GetDaysByTripId(int tripId, bool? isPublic = true);
            IEnumerable<int> GetMyDayIds(int id);
            Task<DayViewModel> PostNewDayAsync(int createdBy, DayPostViewModel newDay);
            Task<DayViewModel> PatchDayAsync(Day day, DayPatchViewModel dayPatch);
            Task<DayViewModel> DeleteDay(Day day);
            List<string> ValidatePost(DayPostViewModel newDay);
            List<string> ValidatePatch(DayPatchViewModel day);
        }

        public interface ILinksService
        {
            IEnumerable<LinkViewModel> GetLinksByName(string name, int createdBy);
            IEnumerable<int> GetMyLinkIds(int id);
            Task<LinkViewModel> PostNewLinkAsync(int createdBy, LinkPostViewModel newLink);
            Task<LinkViewModel> PatchLinkAsync(int id, LinkPatchViewModel link);
            List<string> ValidatePost(LinkPostViewModel newLink);
            List<string> ValidatePatch(LinkPatchViewModel link);
        }

        public interface IAttractionsService
        {
            Attraction FindAttractionById(int id);
            IEnumerable<AttractionViewModel> GetAttractionsByParams(
                string? name,
                long? osmId,
                int? ownerId
            );
            IEnumerable<int> GetMyAttractions(int id);
            Task<AttractionViewModel> PostNewAttractionAsync(
                int? createdBy,
                AttractionPostViewModel newAttraction
            );
            Task<AttractionViewModel> PatchAttractionAsync(
                Attraction attraction,
                AttractionPatchViewModel attractionPatch
            );
            Task<AttractionViewModel> DeleteAttractionAsync(Attraction attraction);
            List<string> ValidatePost(AttractionPostViewModel newAttraction);
            List<string> ValidatePatch(AttractionPatchViewModel attraction);
        }

        public interface IPreferRoutesService
        {
            // prefer routes
            PreferRoute FindPreferRouteById(int id);
            IEnumerable<PreferRouteViewModel> GetPreferRoutesByParams(
                int? type,
                long? departOsmId,
                long? arrivalOsmId,
                int? estimateTimeMin,
                int? estimateTimeMax,
                int? ownerId
            );
            IEnumerable<int> GetMyPreferRoutes(int id);
            Task<PreferRouteViewModel> PostPreferRoutesAsync(
                int createdBy,
                PreferRoutePostViewModel newPreferRoute
            );
            Task<PreferRouteViewModel> PatchPreferRoutesAsync(
                PreferRoute preferRoute,
                PreferRoutePatchViewModel preferRoutePatch
            );
            Task<PreferRouteViewModel> DeletePreferRoute(PreferRoute preferRoute);

            // route types
            RouteType FindRouteTypeById(int id);
            IEnumerable<RouteTypeViewModel> GetAllRouteTypes();
            Task<RouteTypeViewModel> PostNewRouteTypeAsync(string name);
            Task<RouteTypeViewModel> PatchRouteTypeAsync(RouteType routeType, string name);

            // utils
            PreferRouteViewModel ToViewModel(PreferRoute preferRoute);
            List<string> ValidateNameChange(string name);
        }

        public interface ITripAttractionOrdersService
        {
            // taos
            TripAttractionOrder FindTripAttractionOrderById(int id, bool? isPublic = null);
            IEnumerable<TripAttractionOrder> GetTripAttractionOrdersByDayId(int dayId);
            IEnumerable<int> GetMyTripAttractionOrders(int id);
            Task<TripAttractionOrderViewModel> PostTripAttractionOrderAsync(
                int createdBy,
                TripAttractionOrderPostViewModel newTripAttractionOrder
            );
            Task<TripAttractionOrderViewModel> PatchTripAttractionOrderAsync(
                TripAttractionOrder tao,
                TripAttractionOrderPatchViewModel tripAttractionOrder
            );
            Task<IEnumerable<TripAttractionOrderViewModel>> SetOrderAsync(
                TripAttractionOrder tao,
                int newOrder
            );
            Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderAsync(
                TripAttractionOrder tao
            );

            // taors
            TripAttractionOrderRoute FindTripAttractionOrderRoute(int taoId, int preferRouteId);
            Task<TripAttractionOrderViewModel> PostNewTripAttractionOrderRouteAsync(
                int id,
                int preferRouteId,
                int order
            );
            Task<TripAttractionOrderViewModel> SetPreferRouteOrderAsync(
                TripAttractionOrderRoute taor,
                int newOrder
            );
            Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderRouteAsync(
                TripAttractionOrderRoute taor
            );
            bool IsOrderValid(int size, int order);

            // utils
            TripAttractionOrderViewModel ToViewModel(TripAttractionOrder tao);
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
