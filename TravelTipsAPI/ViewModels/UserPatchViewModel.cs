using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels
{
    public class UserPatchViewModel
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }

        public User ToUser()
        {
            var user = new User
            {
                Id = this.Id,
                Username = this.Username,
                Email = this.Email,
            };
            return user;
        }
    }
}
