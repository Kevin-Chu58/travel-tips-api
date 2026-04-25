using TravelTipsAPI.Constants.Enums;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdTargetPostViewModel
    {
        public required string TargetType { get; set; }
        public required string TargetValue { get; set; }
        public int Weight { get; set; }
    }
}
