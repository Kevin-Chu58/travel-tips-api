using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels
{
    public class UserPostViewModel
    {
        public required string UserId { get; set; }

        public User ToUser()
        {
            var user = new User
            {
                Id = new int(),
                UserId = this.UserId,
                Username = "",
                Email = "",
            };
            return user;
        }
    }
}
