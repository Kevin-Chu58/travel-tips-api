namespace TravelTipsAPI.Constants
{
    public class Global
    {
        // User Agent
        public static readonly string USER_AGENT = "TravelTips/0.8";

        // url
        public static readonly string URL_PRODUCTION =
            "https://travel-tips-ui-us-west-g2cxbjaydqejh0af.westus-01.azurewebsites.net";
        public static readonly string URL_LOCALHOST = "http://localhost:5173";

        public static readonly bool IS_PRODUCTION = false;
        public static readonly string URL = IS_PRODUCTION ? URL_PRODUCTION : URL_LOCALHOST;

        // Default Search Result Limit
        public static readonly int USER_DEFAULT_LIMIT = 20;
        public static readonly int TRIP_DEFAULT_LIMIT = 20;
        public static readonly int HIGHLIGHT_DEFAULT_LIMIT = 20;
        public static readonly int BANNER_DEFAULT_LIMIT = 20;
        public static readonly int SUBSCRIPTION_DEFAULT_LIMIT = 20;
    }
}
