using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserPostViewModel
    {
        public required string UserId { get; set; }

        public User ToUser()
        {
            var user = new User
            {
                Id = new int(),
                UserId = UserId,
                Username = "",
                Email = "",
            };
            return user;
        }
    }
}
