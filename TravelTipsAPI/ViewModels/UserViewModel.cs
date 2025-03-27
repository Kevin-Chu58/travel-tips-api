using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }

        public static implicit operator UserViewModel(User? user)
        {
            if (user == null) return user;

            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
            };

            return userViewModel;
        }
    }
}
