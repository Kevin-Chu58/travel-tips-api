using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public string? Picture { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsWriter { get; set; }
        public bool? IsBannerMan { get; set; }

        // stats
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int NumTrips { get; set; }
        public int NumBookmarks { get; set; }

        // relation
        public bool? IsFollowing { get; set; }
    }
}
