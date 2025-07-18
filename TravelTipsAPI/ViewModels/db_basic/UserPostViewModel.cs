using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserPostViewModel
    {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }

        public User ToUser()
        {
            var user = new User
            {
                Id = new int(),
                UserId = UserId,
                Username = Username,
                Email = Email,
            };
            return user;
        }
    }
}
