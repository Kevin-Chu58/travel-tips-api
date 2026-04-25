namespace TravelTipsAPI.ViewModels.db_plan
{
    public class SubscriptionPatchViewModel
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public int? TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? CanceledAt { get; set; }
    }
}
