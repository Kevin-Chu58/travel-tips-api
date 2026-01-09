using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class UserSimpleViewModel
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }

        public static explicit operator UserSimpleViewModel(User user)
        {
            var userSimpleViewModel = new UserSimpleViewModel
            {
                Id = user.Id,
                UserId = user.UserId,
                Username = user.Username ?? "",
                Email = user.Email,
            };

            return userSimpleViewModel;
        }
    }
}
