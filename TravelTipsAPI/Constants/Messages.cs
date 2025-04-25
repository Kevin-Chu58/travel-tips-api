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
    }
}
