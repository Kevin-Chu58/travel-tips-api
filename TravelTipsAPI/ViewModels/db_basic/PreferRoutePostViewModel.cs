using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRoutePostViewModel
    {
        public int Type { get; set; }
        public int DepartOsmId { get; set; }
        public int ArrivalOsmId { get; set; }
        public int EstimateTime { get; set; }
        public int? LinkId { get; set; }

        public PreferRoute ToPreferRoute(int createdBy)
        {
            var newPreferRoute = new PreferRoute
            {
                Id = new int(),
                Type = Type,
                DepartOsmId = DepartOsmId,
                ArrivalOsmId = ArrivalOsmId,
                EstimateTime = EstimateTime,
                LinkId = LinkId,
                CreatedBy = createdBy,
            };

            return newPreferRoute;
        }
    }
}
