namespace TravelTipsAPI.Constants
{
    public class Messages
    {
        // Authorization
        public static readonly string AuthenticationFailed = "Authentication failed.";
        public static readonly string AccessDenied = "Access has been denied.";

        // parameters
        public static readonly string InputInvalid = "Following inputs are invalid: {0}.";

        // Users
        public static readonly string UserNotFound = "User not found";
        public static readonly string UserIdNotFound = "User Auth Id not found.";

        // Trips
        public static readonly string TripNotFound = "Trip not found.";
        public static readonly string TripUnauthorized = "Trip access unauthorized.";

        // Days
        public static readonly string DayNotFound = "Day not found.";
        public static readonly string DayUnauthorized = "Day access unauthorized.";
        public static readonly string DayInputInvalid = "Day input is invalid.";

        //public static readonly string Day24HourRestricted = "Start time cannot equal to End time.";
        //public static readonly string DayStartsBeforeEndRestricted =
        //    "A Day starts before another ends.";

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

        // PreferRoutes
        public static readonly string PreferRouteNotFound = "Prefer Route not found.";
        public static readonly string RouteTypeNotFound = "Route Type not found.";
        public static readonly string EstimateTimeMinMaxRestricted =
            "Maximum estimate time should be greater than the minimum.";
        public static readonly string EstimateTimeRestricted = "Estimate Time should be positive.";
        public static readonly string PreferRouteInUse = "Prefer Route in use.";

        // TripAttractionOrders
        public static readonly string TaoNotFound = "Event not found.";
        public static readonly string TaoTimeInvalid =
            "Event time is not aligned to 15-minute interval.";
        public static readonly string TaoTimeConflicted = "Event time has time conflict.";

        //public static readonly string TaorNotFound = "Trip Attraction Order Route not found.";
        //public static readonly string NewOrderInvalid = "New order is invalid.";
        //public static readonly string TaorExist = "Trip Attraction Order Route exists.";
        //public static readonly string EstimateTravelTimeRestricted =
        //    "Estimate Travel Time should be positive.";

        // Images
        public static readonly string ImageNotFound = "Image not found.";
        public static readonly string ImageMaxAttached = "Maximum number of images attached.";
        public static readonly string ImageUploadFailed =
            "There's a problem when uploading the image.";
        public static readonly string ImageUnauthorized = "Image access unauthorized.";
        public static readonly string ImageStreamEmpty = "Uploaded image stream is empty.";

        // - trip image
        public static readonly string ImageTripAttached = "Image is attached on trip.";
        public static readonly string ImageTripDetached = "Image is not attached on trip.";

        // Nominatim
        public static readonly string OsmEntityNotFound = "Location not found.";

        // HereMap
        public static readonly string HereMapPlaceNotFound = "Location not found.";
    }
}
