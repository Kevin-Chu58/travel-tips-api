using TravelTipsAPI.Constants.Enums;

namespace TravelTipsAPI.ViewModels.Stripe
{
    public class StripeSessionRequest
    {
        public required StripeEnum.Subscription Subscription { get; set; }
    }
}
