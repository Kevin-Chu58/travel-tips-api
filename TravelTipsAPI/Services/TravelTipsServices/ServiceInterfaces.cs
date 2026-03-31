using OpenAI.Batch;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_feed;
using TravelTipsAPI.ViewModels.db_gospel;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.db_plan;
using TravelTipsAPI.ViewModels.db_search;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Constants.Enums.ImageEnum;
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
                out int? followerId,
                int userId,
                GeneralCursor? cursor = null,
                int? limit = null
            );
            IEnumerable<User> GetFollowedUsersByUserIdWithCursor(
                out int? followerId,
                int userId,
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
                out int? lastUserId,
                string username,
                GeneralCursor? cursor = null,
                int? limit = null
            );
            User? GetUserByUserId(string userId);
            User? GetUserByStripeCustomerId(string stripeCustomerId);
            Task<IEnumerable<UserSimpleViewModel>> GetUserSimpleViewModels(IEnumerable<User> users);
            Task<UserViewModel> GetUserViewModelById(int id);
            Task<UserViewModel> UpdateUserAsync(int id, UserPatchViewModel user);
            Task RemoveUserStripeCustomerId(int id);
            Task<bool> AcceptUserAgreementAsync(int id);

            // user profile
            Task<UserProfileViewModel> GetUserProfileViewModel(string auth0Id);

            // user picture
            Task<string?> UpdateUserPicture(User user, ImageViewModel? image);

            // user follower
            Task FollowAsync(int followedId, int followingId);
            Task UnfollowAsync(int followedId, int followingId);
        }

        public interface IUserExtendsService
        {
            UserSubExtend FindUserSubExtendByUserId(int userId);
            Task<UserSubExtend> GetUpdatedUserSubExtendByUserId(int userId);
            Task<UserSubExtend> UpdateSubExtendCycle(
                UserSubExtend userSubExtend,
                DateTimeOffset? subStart,
                int? monthIndex,
                int? subscription = null
            );
            Task UpdateSubExtendNewTripPdf(UserSubExtend userSubExtend);
            Task UpdateSubExtendTripCount(UserSubExtend userSubExtend, int increment);
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
                bool isRestricted = false,
                IEnumerable<int>? editableTripIds = null
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
                int? limit = null,
                bool isMy = false
            );
            Task<TripViewModel> PostNewTripAsync(int createBy, string name);
            Task<TripPatchViewModel> PatchTripAsync(Trip trip, TripPatchViewModel tripPatch);
            Task<List<int>> UpdateIsPublicAsync(int[] tripIds, bool isPublic);
            Task<List<int>> UpdateIsHiddenAsync(int[] tripIds, bool isHidden);
            Task<RegionCompleteViewModel> UpdateRegionAsync(Trip trip, int? regionId);
            Task<int> UpdateBudgetAsync(Trip trip, int? budget);
            Task<int> DeleteTripAsync(Trip trip);
            bool IsOwnerList(int id, int[] tripIds);

            // bookmarks
            Task BookmarkAsync(int userId, int tripId);
            Task UnbookmarkAsync(int userId, int tripId);

            // subscirptions
            IEnumerable<int> GetEditableTripIds(int userId);
            bool CanUserEditTrip(int tripId, int userId);
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

            // subscriptions
            bool CanUserEditDay(int dayId, int userId);
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
            void IsTimeValid(TimeOnly time);
            void IsTaoConflicted(TimeOnly start, TimeOnly end, int dayId, int taoId = 0);

            // subscriptions
            bool CanUserEditTao(int taoId, int userId);
        }
    }

    public class RoleSchema
    {
        public interface IUserRolesService
        {
            bool IsAdmin(int userId);
            bool IsWriter(int userId);
            bool IsBannerMan(int userId);

            // subscriptions
            bool IsUserMember(int userId);
        }
    }

    public class ImageSchema
    {
        public interface IImagesService
        {
            Image? FindImageById(int id);
            Image? FindImageAndBannerCountById(out int bannerCount, int id);
            Task<IEnumerable<ImageViewModel>> GetImagesByIds(int[] ids);
            IEnumerable<int> GetImageIdsByTripId(int id);
            IEnumerable<int> GetImageIdsByUserId(int id);
            IEnumerable<int> GetBannerImageIds();
            Task<ImageViewModel> PostNewImageAsync(
                Stream stream,
                string contentType,
                int userId,
                string? name,
                ImageType? type
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
        public interface IWritingsService
        {
            // writings
            Writing? GetWritingById(int id, bool allowNull = false, bool isRestricted = false);
            Writing? GetWritingByLabelOrder(WritingLabel label, int order);
            int GetWritingOrder(Writing writing);
            Task<IEnumerable<WritingViewModel>> GetWritingsByParams(
                int? createdBy = null,
                string? title = null,
                WritingLabel? label = null,
                bool isRestricted = false,
                bool isDesc = true
            );
            Task<WritingViewModel> GetWritingViewModel(Writing writing, bool hasContent = false);
            IEnumerable<int> GetMyWritings(int userId);
            Task<WritingViewModel> PostWriting(WritingPostViewModel writingPost, int createdBy);
            Task<WritingViewModel> PatchWriting(
                Writing writing,
                WritingPatchViewModel writingPatch
            );
            Task<int> DeleteWriting(Writing writing);

            // writing labels
            WritingLabel? GetLabelById(int id, bool allowNull = false);
            WritingLabel? GetLabelBySlug(string slug);
            IEnumerable<WritingLabelViewModel> GetLabelsByParams(
                string? name = null,
                int? parentLabelId = null,
                string? type = null
            );
            WritingLabelCompleteViewModel? BuildWritingLabelComplete(int? id);
            Task<WritingLabelViewModel> PostNewLabel(
                string name,
                string type,
                int? parentLabelId = null
            );
            Task<WritingLabelViewModel> UpdateLabel(
                WritingLabel label,
                string newName,
                int? parentLabelId = null
            );
            Task<int> DeleteLabel(WritingLabel label);
            bool DoesNameExist(string name);
        }
    }

    public class FeedSchema
    {
        public interface IBannersService
        {
            Banner? FindBannerById(int id);
            Task<BannerViewModel?> GetBannerViewModelById(int id);
            Task<IEnumerable<BannerViewModel>> GetPublicBannerViewModels();
            IEnumerable<BannerSimpleViewModel> GetBanners(
                GeneralCursor? cursor = null,
                int? limit = null
            );
            Task<BannerSimpleViewModel> PostNewBanner(BannerPostViewModel postViewModel);
            Task UpdateBanner(Banner banner, BannerPatchViewModel bannerPatch);
            Task DeleteBanner(Banner banner);

            // styling

            BannerStyling? FindBannerStylingById(int id);
            IEnumerable<BannerStylingSimpleViewModel> GetAllBannerStylings();
            Task<BannerStylingViewModel> PostNewStyling(string name, string styling);
            Task<BannerStylingViewModel> UpdateStyling(
                BannerStyling bannerStyling,
                BannerStylingPatchViewModel bannerStylingPatch
            );
            bool ValidateStyling(string? styling);
        }
    }

    public class PlanSchema
    {
        public interface ISubscriptionsService
        {
            Subscription? FindLastSubscriptionByUserId(int userId);
            Subscription? FindActiveSubscriptionByUserId(int userId);
            SubscriptionViewModel? GetActiveSubscriptionByUserId(int userId);
            IEnumerable<SubscriptionViewModel> GetSubscriptionsByUserIdWithCursor(
                int userId,
                GeneralCursor? cursor = null,
                int? limit = null
            );
            Task AddSubscription(SubscriptionPostViewModel newSubscription);
            Task UpdateSubscription(
                Subscription subscription,
                SubscriptionPatchViewModel subscriptionPatch
            );
            Task ExpireActiveSubscriptionByUserId(int userId);

            // subscription status
            Task UpdateSubscriptionStatus(string subId, bool cancelSub);
        }
    }
}
