namespace TravelTipsAPI.Constants
{
    public class Messages
    {
        // Authorization
        public static readonly string AuthenticationFailed = "Authentication failed.";
        public static readonly string AccessDenied = "Access has been denied.";
        public static readonly string EmailUnverified = "Email is not verified.";

        // parameters
        public static readonly string InputInvalid = "Following inputs are invalid: {0}.";

        // Cursor
        public static readonly string CursorInvalid = "Cursor is invalid.";

        // Users
        public static readonly string UserNotFound = "User not found";
        public static readonly string UserIdNotFound = "User Auth Id not found.";

        // User Extends
        // - user sub extends
        public static readonly string UserSubExtendNotFound =
            "User Subscription Statistics not found.";
        public static readonly string MonthlyPdfGenerationLimitReached =
            "User monthly pdf generation limit is reached.";

        // Regions
        public static readonly string RegionNotFound = "Region not found.";
        public static readonly string RegionInvalid = "Region invalid.";

        // Bookmarks
        public static readonly string BookmarkNotFound = "Bookmark not found.";
        public static readonly string BookmarkAlreadyExists = "Bookmark already exists.";

        // Followers
        public static readonly string FollowNotFound = "Follow relationship not found.";
        public static readonly string FollowAlreadyExists = "Follow relationship already exists.";
        public static readonly string FollowSelf = "Cannot follow yourself.";

        // Trips
        public static readonly string TripNotFound = "Trip not found.";
        public static readonly string TripUnauthorized = "Trip access unauthorized.";
        public static readonly string TripBudgetInvalid = "Trip budget is invalid.";

        // TripShares
        public static readonly string TripShareNotFound = "Trip share not found.";
        public static readonly string TripAlreadyShared = "Trip already shared with the user.";
        public static readonly string TripShareWithSelf = "Cannot share trip with yourself.";
        public static readonly string TripUnshareWithSelf = "Cannot unshare trip with yourself.";

        // Days
        public static readonly string DayNotFound = "Day not found.";
        public static readonly string DayUnauthorized = "Day access unauthorized.";
        public static readonly string DayInputInvalid = "Day input is invalid.";
        public static readonly string DayMaxReached = "Maximum number of days created.";

        // Links
        public static readonly string LinkNotFound = "Link not found.";
        public static readonly string LinkInvalid = "Link is invalid.";

        // Attractions
        public static readonly string AttractionNotFound = "Attraction not found.";
        public static readonly string HighlightUnauthorized = "Highlight access unauthorized.";
        public static readonly string OsmIdRestricted = "Osm Id should be positive.";
        public static readonly string OsmTypeInvalid = "Osm Type is invalid";

        // Highlights
        public static readonly string HighlightNotFound = "Highlight not found.";
        public static readonly string HighlightDescriptionEmpty = "Highlight is empty";

        // PreferRoutes
        public static readonly string PreferRouteNotFound = "Prefer Route not found.";
        public static readonly string RouteTypeNotFound = "Route Type not found.";
        public static readonly string EstimateTimeMinMaxRestricted =
            "Maximum estimate time should be greater than the minimum.";
        public static readonly string EstimateTimeRestricted = "Estimate Time should be positive.";
        public static readonly string PreferRouteInUse = "Prefer Route in use.";

        // TripAttractionOrders
        public static readonly string TaoNotFound = "Event not found.";
        public static readonly string TaoUnauthorized = "Event access unauthorized.";
        public static readonly string TaoTimeInvalid =
            "Event time is not aligned to 15-minute interval.";
        public static readonly string TaoTimeConflicted = "Event has time conflict.";
        public static readonly string TaoMaxReached = "Maximum number of events created.";

        // Images
        public static readonly string ImageNotFound = "Image not found.";
        public static readonly string ImageMaxAttached = "Maximum number of images attached.";
        public static readonly string ImageUploadFailed =
            "There's a problem when uploading the image.";
        public static readonly string ImageUnauthorized = "Image access unauthorized.";
        public static readonly string ImageStreamEmpty = "Uploaded image stream is empty.";
        public static readonly string ImageNameTooLong = "Image name too long.";

        // - trip image
        public static readonly string ImageTripAttached = "Image is attached to trip.";
        public static readonly string ImageTripDetached = "Image is not attached to trip.";

        // - banner image
        public static readonly string ImageBannerAttached = "Image is attached to banner.";

        // - user image (picture)
        public static readonly string ImageUserPicture = "Image is profile picture.";

        // HereMap
        public static readonly string HereMapPlaceNotFound = "Location not found.";
        public static readonly string HereMapRouteNotFound = "Route not found.";
        public static readonly string HereMapTransportModeNotFound = "Transport mode not found.";

        // Wiki Commons
        public static readonly string WikiCommonsQueryNotFound = "WikiImage:Query not found.";
        public static readonly string WikiCommonsPagesNotFound = "WikiImage:Pages not found.";

        // Writings
        public static readonly string WritingNotFound = "Writing not found.";
        public static readonly string WritingUnauthorized = "Writing unauthorized.";

        // - writing labels
        public static readonly string WritingLabelNotFound = "Writing label not found";
        public static readonly string WritingLabelTypeInvalid = "Writing label type is invalid.";
        public static readonly string WritingLabelExists = "Writing label already exists.";

        // banners
        public static readonly string BannerNotFound = "Banner not found.";

        // - banner stylings
        public static readonly string BannerStylingNotFound = "Banner styling not found.";
        public static readonly string BannerStylingInvalid = "Banner styling is invalid.";

        // memberships
        public static readonly string MembershipRequired = "You do not have membership.";

        // Stripe
        public static readonly string StripeSessionNotFound = "Stripe session not found.";

        // subscriptions
        public static readonly string SubscriptionTypeInvalid = "Invalid subscription type.";
        public static readonly string SubscriptionAlreadyActive =
            "User already has an active subscription.";
        public static readonly string SubscriptionNotFound = "Subscription not found.";
    }
}
