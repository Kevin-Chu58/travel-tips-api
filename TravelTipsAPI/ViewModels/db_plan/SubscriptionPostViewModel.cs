namespace TravelTipsAPI.ViewModels.db_plan
{
    public class SubscriptionPostViewModel
    {
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int TotalAmount { get; set; }
        public required string StripeSubscriptionId { get; set; }
    }
}
