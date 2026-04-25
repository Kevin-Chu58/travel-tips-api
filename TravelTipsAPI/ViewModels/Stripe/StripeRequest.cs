using System.ComponentModel.DataAnnotations;
using TravelTipsAPI.Constants.Enums;
using static TravelTipsAPI.Constants.Enums.StripeEnum;

namespace TravelTipsAPI.ViewModels.Stripe
{
    public class StripeRequest
    {
        public required SubscriptionEnum Subscription { get; set; }
    }

    public class StripeAdWeightRequest
    {
        [MinLength(1)]
        [MaxLength(10)]
        public required string TargetType { get; set; }

        [MinLength(1)]
        [MaxLength(100)]
        public required string TargetValue { get; set; }
        public int Weight { get; set; }
    }
}
