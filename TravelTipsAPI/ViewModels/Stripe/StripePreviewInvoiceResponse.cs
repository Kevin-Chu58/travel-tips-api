using Stripe;

namespace TravelTipsAPI.ViewModels.Stripe
{
    public class StripePreviewInvoiceResponse
    {
        public required string Currency { get; set; }
        public required long AmountToPayNow { get; set; }
        public required long NextCycleTotal { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class StripeBillingCyclePreviewInvoiceResponse
    {
        public required string Currency { get; set; }
        public required long NextBillingAmount { get; set; }
        public DateTime NextBillingDate { get; set; }
    }
}
