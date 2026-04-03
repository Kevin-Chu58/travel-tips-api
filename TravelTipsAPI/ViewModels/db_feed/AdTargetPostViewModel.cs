using TravelTipsAPI.Constants.Enums;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdTargetPostViewModel
    {
        public required AdTargetEnum.AdTarget TargetType { get; set; }
        public required string TargetValue { get; set; }
        public required string StripeItemId { get; set; }
        public int Weight { get; set; }
    }
}
