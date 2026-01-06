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
            User? GetUserByUserId(string userId);
            Task<User?> GetUserByUserIdAsync(string userId);
            Task<UserViewModel> PostNewUserAsync(UserPostViewModel userPost);
            Task<UserViewModel> UpdateUserAsync(int id, UserPatchViewModel newUser);
            Task<bool> AcceptUserAgreementAsync(int id);
        }

        public interface ITripsService
        {
            IEnumerable<int> GetMyTripIds(int id);
            Trip FindTripByParams(int id, bool? isPublic = null);
            IEnumerable<TripViewModel> GetTripsByTitle(string title);
            TripViewModel GetTripByTripId(int id);
            IEnumerable<TripViewModel> GetTripsByUserId(int id);
            Trip? GetTripByDayId(int dayId);
            Task<TripViewModel> PostNewTripAsync(int createBy, string name);
            Task<TripPatchViewModel> PatchTripAsync(Trip trip, TripPatchViewModel tripPatch);
            Task<List<int>> UpdateIsPublicAsync(int[] tripIds, bool isPublic);
            Task<List<int>> UpdateIsHiddenAsync(int[] tripIds, bool isHidden);
            Task<RegionCompleteViewModel?> UpdateRegionAsync(Trip trip, int? regionId);
            Task<int?> UpdateBudgetAsync(Trip trip, int? budget);
            bool IsOwnerList(int id, int[] tripIds);
            List<string> ValidatePost(string name);
            List<string> ValidatePatch(TripPatchViewModel trip);
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
            Attraction FindAttractionByHereId(string hereId);
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
            Highlight FindHighlightById(int id);
            IEnumerable<Highlight> GetHighlightsByParams(int id, int? userId);
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
            TripAttractionOrder? FindTaoById(int id);
            TripAttractionOrderViewModel GetTaoById(int id);
            IEnumerable<TripAttractionOrderViewModel> GetTaosByDayId(int dayId);
            IEnumerable<TripAttractionOrderGeoViewModel> GetTaoGeosByDayId(int dayId);
            IEnumerable<TripAttractionOrderGeoViewModel> GetTaoGeosByTripId(int tripId);
            HereRoutingInput? GetHereRoutingInputByTaoId(int taoId);
            IEnumerable<HereRouting> GetAttractionRoutingsByDayId(int dayId);
            Task<int> PostTao(TripAttractionOrderPostViewModel newTao, int userId);
            Task<int> PatchTao(TripAttractionOrderPatchViewModel taoPatch, TripAttractionOrder tao);
            Task<int> PatchTaoDetachHighlight(TripAttractionOrder tao);
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
