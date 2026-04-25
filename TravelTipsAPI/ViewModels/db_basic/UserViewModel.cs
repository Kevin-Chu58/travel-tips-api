using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public string? Picture { get; set; }
        public required string Email { get; set; }
        public bool UserAgreement { get; set; }
        public bool EmailVerified { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsWriter { get; set; }
        public bool? IsBannerMan { get; set; }
        public bool? IsReviewer { get; set; }
        public bool RenewSubscription { get; set; }
        public string? StripeCustomerId { get; set; }
        public UserSubExtendViewModel? UserSubExtend { get; set; }
    }
}
