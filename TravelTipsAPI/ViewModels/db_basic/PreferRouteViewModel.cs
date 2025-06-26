using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRouteViewModel
    {
        public int Id { get; set; }
        public bool IsDeprecated { get; set; }
        public RouteTypeViewModel? Type { get; set; }
        public long ArrivalOsmId { get; set; }
        public int EstimateTime { get; set; }
        public int? LinkId { get; set; }
        public int CreatedBy { get; set; }

        // to be filled outside
        public AttractionViewModel? DepartAttraction { get; set; }
        public AttractionViewModel? ArrivalAttraction { get; set; }

        public static explicit operator PreferRouteViewModel(PreferRoute preferRoute)
        {
            var preferRouteViewModel = new PreferRouteViewModel
            {
                Id = preferRoute.Id,
                IsDeprecated = preferRoute.IsDeprecated,
                EstimateTime = preferRoute.EstimateTime,
                LinkId = preferRoute.LinkId,
                CreatedBy = preferRoute.CreatedBy,
            };

            return preferRouteViewModel;
        }
    }
}
