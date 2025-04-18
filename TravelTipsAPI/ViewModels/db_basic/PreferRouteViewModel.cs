using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRouteViewModel
    {
        public int Id { get; set; }
        public RouteTypeViewModel? Type { get; set; }
        public required string Ref {  get; set; }
        public int DepartOsmId { get; set; }
        public int ArrivalOsmId { get; set; }
        public int EstimateTime { get; set; }
        public int? LinkId { get; set; }
        public int CreatedBy { get; set; }

        public static explicit operator PreferRouteViewModel?(PreferRoute? preferRoute)
        {
            if (preferRoute == null) return null;

            var preferRouteViewModel = new PreferRouteViewModel
            {
                Id = preferRoute.Id,
                Ref = preferRoute.Ref,
                DepartOsmId = preferRoute.DepartOsmId,
                ArrivalOsmId = preferRoute.ArrivalOsmId,
                EstimateTime = preferRoute.EstimateTime,
                LinkId = preferRoute.LinkId,
                CreatedBy = preferRoute.CreatedBy
            };

            return preferRouteViewModel;
        }
    }
}
