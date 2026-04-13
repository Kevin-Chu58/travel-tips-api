namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserPatchViewModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool? RenewSubscription { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? StripeCurrency { get; set; }
    }
}
