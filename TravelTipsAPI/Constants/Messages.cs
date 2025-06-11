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
        public static readonly string UserIdNotFound = "User Auth Id not found.";

        // Trips
        public static readonly string TripNotFound = "Trip not found.";
        public static readonly string TripUnauthorized = "Trip access unauthorized.";

        // Days
        public static readonly string DayNotFound = "Day not found.";
        public static readonly string Day24HourRestricted = "Start time cannot equal to End time.";
        public static readonly string DayStartsBeforeEndRestricted =
            "A Day starts before another ends.";

        // Links
        public static readonly string LinkNotFound = "Link not found.";
        public static readonly string LinkInvalid = "Link is invalid.";

        // Attractions && Highlights
        public static readonly string AttractionNotFound = "Attraction not found.";
        public static readonly string HighlightUnauthorized = "Highlight access unauthorized.";
        public static readonly string OsmIdRestricted = "Osm Id should be positive.";
        public static readonly string OsmTypeInvalid = "Osm Type is invalid";

        // PreferRoutes
        public static readonly string PreferRouteNotFound = "Prefer Route not found.";
        public static readonly string RouteTypeNotFound = "Route Type not found.";
        public static readonly string EstimateTimeMinMaxRestricted =
            "Maximum estimate time should be greater than the minimum.";
        public static readonly string EstimateTimeRestricted = "Estimate Time should be positive.";
        public static readonly string PreferRouteInUse = "Prefer Route in use.";

        // TripAttractionOrders
        public static readonly string TaoNotFound = "Trip Attraction Order not found.";
        public static readonly string TaorNotFound = "Trip Attraction Order Route not found.";
        public static readonly string NewOrderInvalid = "New order is invalid.";
        public static readonly string TaorExist = "Trip Attraction Order Route exists.";
        public static readonly string EstimateTravelTimeRestricted =
            "Estimate Travel Time should be positive.";
    }
}
