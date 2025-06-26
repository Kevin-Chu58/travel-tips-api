using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRoutePostViewModel
    {
        public int Type { get; set; }
        public required AttractionViewModel DepartAttraction { get; set; }
        public required AttractionViewModel ArrivalAttraction { get; set; }
        public int EstimateTime { get; set; }
        public int? LinkId { get; set; }

        public PreferRoute ToPreferRoute(int createdBy)
        {
            var newPreferRoute = new PreferRoute
            {
                Id = new int(),
                Type = Type,
                DepartAttractionId = DepartAttraction.Id ?? new int(),
                ArrivalAttractionId = ArrivalAttraction.Id ?? new int(),
                EstimateTime = EstimateTime,
                LinkId = LinkId,
                CreatedBy = createdBy,
            };

            return newPreferRoute;
        }
    }
}
