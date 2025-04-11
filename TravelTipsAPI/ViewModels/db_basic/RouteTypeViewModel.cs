using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class RouteTypeViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public static explicit operator RouteTypeViewModel?(RouteType? routeType)
        {
            if (routeType == null) return null;

            var routeTypeViewModel = new RouteTypeViewModel
            {
                Id = routeType.Id,
                Name = routeType.Name
            };

            return routeTypeViewModel;
        }
    }
}
