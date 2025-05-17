using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRouteViewModel
    {
        public int Id { get; set; }
        public RouteTypeViewModel? Type { get; set; }
        public long DepartOsmId { get; set; }
        public long ArrivalOsmId { get; set; }
        public int EstimateTime { get; set; }
        public int? LinkId { get; set; }
        public int CreatedBy { get; set; }

        public static explicit operator PreferRouteViewModel(PreferRoute preferRoute)
        {
            var preferRouteViewModel = new PreferRouteViewModel
            {
                Id = preferRoute.Id,
                DepartOsmId = preferRoute.DepartOsmId,
                ArrivalOsmId = preferRoute.ArrivalOsmId,
                EstimateTime = preferRoute.EstimateTime,
                LinkId = preferRoute.LinkId,
                CreatedBy = preferRoute.CreatedBy,
            };

            return preferRouteViewModel;
        }
    }
}
