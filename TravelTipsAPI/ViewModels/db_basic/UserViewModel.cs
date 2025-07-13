using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public required string Username { get; set; }

        public static explicit operator UserViewModel(User user)
        {
            var userViewModel = new UserViewModel { Id = user.Id, Username = user.Username };

            return userViewModel;
        }
    }
}
