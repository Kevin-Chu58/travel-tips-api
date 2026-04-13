namespace TravelTipsAPI.ViewModels.db_plan
{
    public class SubscriptionViewModel
    {
        public int Id { get; set; }
        public required string Plan { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int TotalAmount { get; set; }
        public required string StripeSubscriptionId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CanceledAt { get; set; }
    }
}
