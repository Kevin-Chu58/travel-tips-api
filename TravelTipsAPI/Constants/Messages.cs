namespace TravelTipsAPI.Constants
{
    public class Messages
    {
        // Authorization
        public static readonly string AuthenticationFailed = "Authentication failed.";
        public static readonly string AccessDenied = "Access has been denied.";

        // parameters
        public static readonly string InputInvalid = "Following inputs are invalid: {0}.";

        // Cursor
        public static readonly string CursorInvalid = "Cursor is invalid.";

        // Users
        public static readonly string UserNotFound = "User not found";
        public static readonly string UserIdNotFound = "User Auth Id not found.";

        // Regions
        public static readonly string RegionNotFound = "Region not found.";
        public static readonly string RegionInvalid = "Region invalid.";

        //public static readonly string RegionRootInvalid = "Region hierarchy is missing Country";

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
        public static readonly string ImageTripAttached = "Image is attached on trip.";
        public static readonly string ImageTripDetached = "Image is not attached on trip.";

        // - user image (picture)
        public static readonly string ImageUserPicture = "Image is profile picture.";

        // HereMap
        public static readonly string HereMapPlaceNotFound = "Location not found.";
        public static readonly string HereMapRouteNotFound = "Route not found.";
        public static readonly string HereMapTransportModeNotFound = "Transport mode not found.";

        // Wiki Commons
        public static readonly string WikiCommonsQueryNotFound = "WikiImage:Query not found.";
        public static readonly string WikiCommonsPagesNotFound = "WikiImage:Pages not found.";

        // Sermons
        public static readonly string SermonNotFound = "Sermon not found.";
        public static readonly string SermonUnauthorized = "Sermon unauthorized.";

        // - sermon labels
        public static readonly string SermonLabelNotFound = "Sermon label not found";
        public static readonly string SermonLabelTypeInvalid = "Sermon label type is invalid.";
        public static readonly string SermonLabelExists = "Sermon label already exists.";
    }
}
