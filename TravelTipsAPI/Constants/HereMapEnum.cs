namespace TravelTipsAPI.Constants
{
    public class HereMapEnum
    {
        public enum RouteMode
        {
            Car,
            Truck,
            Pedestrian,
            PublicTransit,
        };

        public static readonly Dictionary<string, RouteMode> ModeMap = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "car", RouteMode.Car },
            { "truck", RouteMode.Truck },
            { "pedestrian", RouteMode.Pedestrian },
            { "public transit", RouteMode.PublicTransit },
        };
    }
}
