namespace TravelTipsAPI.ViewModels.Stripe
{
    public class StripePreviewInvoiceResponse
    {
        public required long AmountToPayNow { get; set; }
        public required long NextCycleTotal { get; set; }
        public DateTime StartDate { get; set; }
    }
}
