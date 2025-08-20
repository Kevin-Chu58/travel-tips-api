namespace TravelTipsAPI.Constants
{
    public class HereMapEnum
    {
        public enum RouteMode
        {
            Car,
            Truck,
            Pedistrian,
            PublicTransit,
        };

        public static readonly Dictionary<string, RouteMode> ModeMap = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "car", RouteMode.Car },
            { "truck", RouteMode.Truck },
            { "pedistrian", RouteMode.Pedistrian },
            { "public transit", RouteMode.PublicTransit },
        };
    }
}
