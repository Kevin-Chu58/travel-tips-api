using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_gospel;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.db_search;
using TravelTipsAPI.ViewModels.db_sermon;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Constants.OrderBy.HighlightOrderBy;
using static TravelTipsAPI.Constants.OrderBy.TripOrderBy;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

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
            RegionViewModel GetRegionByCountryAndState(string countrySlug, string? stateSlug);
            RegionCompleteViewModel BuildRegionComplete(int regionId);
        }

        public interface IBookmarksService
        {
            IEnumerable<int> GetBookmarkTripIdsByUserId(int userId);
            Task AddBookmarkAsync(int userId, int tripId);
            Task RemoveBookmarkAsync(int userId, int tripId);
        }

        public interface IFollowersService
        {
            IEnumerable<User> GetFollowingUsersByUserIdWithCursor(
                int userId,
                out int? followerId,
                GeneralCursor? cursor = null,
                int? limit = null
            );
            IEnumerable<User> GetFollowedUsersByUserIdWithCursor(
                int userId,
                out int? followerId,
                GeneralCursor? cursor = null,
                int? limit = null
            );
            bool IsFollowing(int followedId, int followingId);
            Task FollowUserAsync(int followedId, int followingId);
            Task UnfollowUserAsync(int followedId, int followingId);
        }
    }

    public class BasicSchema
    {
        public interface IUsersService
        {
            User GetUserById(int id);
            IEnumerable<User> GetUsersByIds(IEnumerable<int> ids);
            IEnumerable<User> GetUsersByUsernameWithCursor(
                string username,
                GeneralCursor? cursor = null,
                int? limit = null
            );
            User? GetUserByUserId(string userId);
            Task<IEnumerable<UserSimpleViewModel>> GetUserSimpleViewModels(IEnumerable<User> users);
            Task<IEnumerable<UserViewModel>> GetUserViewModels(IEnumerable<User> users);
            Task<UserViewModel> UpdateUserAsync(int id, UserPatchViewModel newUser);
            Task<bool> AcceptUserAgreementAsync(int id);

            // user profile
            Task<UserProfileViewModel> GetUserProfileViewModel(string auth0Id);

            // user picture
            Task<string?> UpdateUserPicture(User user, ImageViewModel? image);

            // user follower
            Task FollowAsync(int followedId, int followingId);
            Task UnfollowAsync(int followedId, int followingId);
        }

        public interface ITripsService
        {
            Trip? FindTripByParams(int? id = null, int? dayId = null, bool? isPublic = null);
            Task<TripViewModel?> GetTripById(int id, int? userId = null, bool isRestricted = false);
            Task<TripViewModel> GetTripViewModel(
                Trip trip,
                int? userId = null,
                IEnumerable<UserSimpleViewModel>? users = null,
                Dictionary<int, int>? dayCounts = null,
                bool isRestricted = false
            );
            IEnumerable<int> GetMyTripIds(int id);
            Task<IEnumerable<TripViewModel>> GetTripsByParams(
                int? userId = null,
                IEnumerable<int>? ids = null,
                string? title = null,
                int? createdBy = null,
                bool? isPublic = null,
                bool? isHidden = null,
                RegionViewModel? region = null,
                int? budget = null,
                bool isRestricted = false,
                TripCursor? cursor = null,
                TripOrderByEnum? tripOrderByEnum = null,
                int? limit = null
            );
            Task<TripViewModel> PostNewTripAsync(int createBy, string name);
            Task<TripPatchViewModel> PatchTripAsync(Trip trip, TripPatchViewModel tripPatch);
            Task<List<int>> UpdateIsPublicAsync(int[] tripIds, bool isPublic);
            Task<List<int>> UpdateIsHiddenAsync(int[] tripIds, bool isHidden);
            Task<RegionCompleteViewModel> UpdateRegionAsync(Trip trip, int? regionId);
            Task<int> UpdateBudgetAsync(Trip trip, int? budget);
            bool IsOwnerList(int id, int[] tripIds);

            // bookmarks
            Task BookmarkAsync(int userId, int tripId);
            Task UnbookmarkAsync(int userId, int tripId);
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
            Task<IEnumerable<HighlightViewModel>> GetHighlightsByParams(
                int? attractionId = null,
                int? createdBy = null,
                HighlightCursor? cursor = null,
                HighlightOrderByEnum? highlightOrderByEnum = null,
                int? limit = null
            );
            Task<HighlightViewModel> GetHighlightViewModel(
                Highlight highlight,
                IEnumerable<UserSimpleViewModel>? users = null,
                bool getUserPic = false
            );
            IEnumerable<int> GetMyHighlights(int id);
            Task<HighlightViewModel> PostNewHighlightAsync(
                HighlightPostViewModel newHighlight,
                int userId
            );
            Task<HighlightViewModel> UpdateHighlightAsync(Highlight highlight, string description);
            Task UpdateHighlightUsageCountAsync(int? oldId, int? newId);
            Task<int> DeleteHighlightAsync(Highlight highlight);
        }

        public interface ITripAttractionOrdersService
        {
            IEnumerable<int> GetMyTaos(int id);
            int BackTrackTripIdByTaoId(int taoId);
            TripAttractionOrder? FindTaoById(int id);
            Task<TripAttractionOrderViewModel> GetTaoById(
                int id,
                bool isRestricted = false,
                bool getUserPic = false
            );
            Task<IEnumerable<TripAttractionOrderViewModel>> GetTaosByDayId(
                int dayId,
                bool isRestricted = false,
                bool getUserPic = false
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
            bool IsWriter(int userId);
        }
    }

    public class ImageSchema
    {
        public interface IImagesService
        {
            Image? FindImageById(int id);
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

    public class GospelSchema
    {
        public interface ISermonsService
        {
            // sermons
            Sermon? GetSermonById(int id, bool allowNull = false, bool isRestricted = false);
            Sermon? GetSermonByLabelOrder(SermonLabel label, int order);
            int GetSermonOrder(Sermon sermon);
            Task<IEnumerable<SermonViewModel>> GetLatestSermons();
            Task<IEnumerable<SermonViewModel>> GetSermonsByParams(
                int? createdBy = null,
                string? title = null,
                SermonLabel? label = null,
                bool? isBanner = null,
                bool isRestricted = false,
                bool isDesc = true
            );
            Task<SermonViewModel> GetSermonViewModel(Sermon sermon, bool hasContent = false);
            IEnumerable<int> GetMySermons(int userId);
            Task<SermonViewModel> PostSermon(SermonPostViewModel sermonPost, int createdBy);
            Task<SermonViewModel> PatchSermon(Sermon sermon, SermonPatchViewModel sermonPatch);
            Task<int> DeleteSermon(Sermon sermon);

            // sermon labels
            SermonLabel? GetLabelById(int id, bool allowNull = false);
            SermonLabel? GetLabelBySlug(string slug);
            IEnumerable<SermonLabelViewModel> GetLabelsByParams(
                string? name = null,
                int? parentLabelId = null,
                string? type = null
            );
            SermonLabelCompleteViewModel? BuildSermonLabelComplete(int? id);
            Task<SermonLabelViewModel> PostNewLabel(
                string name,
                string type,
                int? parentLabelId = null
            );
            Task<SermonLabelViewModel> UpdateLabel(
                SermonLabel label,
                string newName,
                int? parentLabelId = null
            );
            Task<int> DeleteLabel(SermonLabel label);
            bool DoesNameExist(string name);
        }
    }
}
