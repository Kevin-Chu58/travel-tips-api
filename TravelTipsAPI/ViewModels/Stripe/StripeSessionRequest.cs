using TravelTipsAPI.Constants;

namespace TravelTipsAPI.ViewModels.Stripe
{
    public class StripeSessionRequest
    {
        public required StripeEnum.Subscription Subscription { get; set; }
    }
}
