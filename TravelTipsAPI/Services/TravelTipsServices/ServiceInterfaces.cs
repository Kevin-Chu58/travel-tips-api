using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.db_search;
using TravelTipsAPI.ViewModels.HereMap;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    public class SearchSchema
    {
        public interface IRegionsService
        {
            RegionViewModel GetRegionById(int id);
            RegionViewModel GetRegionByName(string name);
            IEnumerable<RegionViewModel> GetRegionsByParams(
                string type,
                string? name = null,
                int? parentRegionId = null
            );
            RegionCompleteViewModel BuildRegionComplete(int regionId);
        }
    }

    public class BasicSchema
    {
        public interface IUsersService
        {
            User GetUserById(int id);
            IEnumerable<User> GetUsersByIds(IEnumerable<int> ids);
            User? GetUserByUserId(string userId);
            Task<UserViewModel> UpdateUserAsync(int id, UserPatchViewModel newUser);
            Task<bool> AcceptUserAgreementAsync(int id);
        }

        public interface ITripsService
        {
            Trip? FindTripByParams(int? id = null, int? dayId = null, bool? isPublic = null);
            TripViewModel? GetTripById(int id, bool isRestricted = false);
            TripViewModel GetTripViewModel(Trip trip, bool isRestricted = false);
            IEnumerable<int> GetMyTripIds(int id);
            IEnumerable<TripViewModel> GetTripsByParams(
                IEnumerable<int>? ids = null,
                string? title = null,
                int? userId = null,
                bool? isPublic = null,
                bool? isHidden = null,
                int? regionId = null,
                int? budget = null,
                bool isRestricted = false
            );
            Task<TripViewModel> PostNewTripAsync(int createBy, string name);
            Task<TripPatchViewModel> PatchTripAsync(Trip trip, TripPatchViewModel tripPatch);
            Task<List<int>> UpdateIsPublicAsync(int[] tripIds, bool isPublic);
            Task<List<int>> UpdateIsHiddenAsync(int[] tripIds, bool isHidden);
            Task<RegionCompleteViewModel> UpdateRegionAsync(Trip trip, int? regionId);
            Task<int> UpdateBudgetAsync(Trip trip, int? budget);
            bool IsOwnerList(int id, int[] tripIds);
        }

        public interface ITripSharesService
        {
            TripShare? FindTripShare(int tripId, int userId);
            IEnumerable<TripShare> FindTripSharesByTripId(int tripId);
            IEnumerable<int> GetSharedUserIdsByTripId(int tripId);
            IEnumerable<int> GetSharedTripIdsByUserId(int userId);
            bool IsTripSharedWithUser(int tripId, int userId);
            Task ShareTripWithUser(int tripId, int userId);
            Task UnshareTripWithUser(int tripId, int userId);
            Task<int> UnshareTripWithAll(int tripId);
        }

        public interface IDaysService
        {
            IEnumerable<int> GetMyDayIds(int id);
            Day FindDayById(int id, bool? isPublic = null);
            IEnumerable<DayViewModel> GetDaysByTripId(int tripId);
            Task<DayViewModel> PostNewDayAsync(int createdBy, int tripId);
            Task<DayViewModel> PatchDayAsync(Day day, DayPatchViewModel dayPatch);
            Task<DayViewModel> DeleteDay(Day day);
        }

        public interface IAttractionsService
        {
            Attraction FindAttractionById(int id);
            IEnumerable<AttractionViewModel> GetAttractionsByParams(
                string? title,
                string? Category = null,
                string? ResultType = null,
                string? HereId = null,
                string? City = null,
                string? State = null,
                string? Country = null,
                int? ownerId = null
            );
            IEnumerable<int> GetMyHighlights(int id);
            Task<Attraction> PostNewAttractionAsync(Attraction newAttraction);
            Task<Attraction> UpdateAttractionAsync(Attraction attraction, Attraction newAttraction);
        }

        public interface IHighlightsService
        {
            Highlight? FindHighlightById(int id);
            IEnumerable<Highlight> GetHighlightsByParams(int id, int? userId = null);
            HighlightViewModel GetHighlightViewModel(Highlight highlight);
            IEnumerable<int> GetMyHighlights(int id);
            Task<HighlightViewModel> PostNewHighlightAsync(
                HighlightPostViewModel newHighlight,
                int userId
            );
            Task<HighlightViewModel> UpdateHighlightAsync(Highlight highlight, string description);
            Task<HighlightViewModel> DeleteHighlightAsync(Highlight highlight);
        }

        public interface ITripAttractionOrdersService
        {
            IEnumerable<int> GetMyTaos(int id);
            int BackTrackTripIdByTaoId(int taoId);
            TripAttractionOrder? FindTaoById(int id);
            TripAttractionOrderViewModel GetTaoById(int id, bool isRestricted = false);
            IEnumerable<TripAttractionOrderViewModel> GetTaosByDayId(
                int dayId,
                bool isRestricted = false
            );
            IEnumerable<TripAttractionOrderGeoViewModel> GetTaoGeosByDayId(
                int dayId,
                bool isRestricted = false
            );
            IEnumerable<TripAttractionOrderGeoViewModel> GetTaoGeosByTripId(
                int tripId,
                bool isRestricted = false
            );
            HereRoutingInput? GetHereRoutingInputByTaoId(int taoId, bool isRestricted = false);
            IEnumerable<HereRouting> GetAttractionRoutingsByDayId(
                int dayIdbool,
                bool isRestricted = false
            );
            Task<int> PostTao(TripAttractionOrderPostViewModel newTao, int userId);
            Task<int> PatchTao(TripAttractionOrderPatchViewModel taoPatch, TripAttractionOrder tao);
            Task<int> PatchTaoDetachHighlight(TripAttractionOrder tao);
            Task<bool> PatchTaoSetPrivate(TripAttractionOrder tao, bool isPrivate);
            Task<int> DeleteTaoById(TripAttractionOrder tao);
            Task<int> DeleteTaosByDayId(int dayId);
            void IsTimeValid(TimeOnly time);
            void IsTaoConflicted(TimeOnly start, TimeOnly end, int dayId, int taoId = 0);
        }
    }

    public class RoleSchema
    {
        public interface IUserRolesService
        {
            bool IsAdmin(int userId);
        }
    }

    public class ImageSchema
    {
        public interface IImagesService
        {
            Image? GetImageById(int id);
            Task<IEnumerable<ImageViewModel>> GetImagesByIds(int[] ids);
            IEnumerable<int> GetImageIdsByUserId(int id);
            IEnumerable<int> GetImageIdsByTripId(int id);
            Task<ImageViewModel> PostNewImageAsync(
                Stream stream,
                string contentType,
                int userId,
                string? name
            );
            Task<byte[]> DownloadImageAsync(int userId, Guid guid);
            Task UpdateImageName(Image image, string newName);
            Task<ImageRelationViewModel> AttachImageToTrip(int imageId, int tripId);
            Task<ImageRelationViewModel> DetachImageFromTrip(int imageId, int tripId);
            Task DeleteImageAsync(Image image);
            bool IsOwner(int userId, int imageId);
        }
    }
}
