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

        // stats
        public int NumTrips { get; set; }
        public int NumBookmarks { get; set; }
    }
}
