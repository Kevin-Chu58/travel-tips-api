using System.Threading.Tasks;
using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Services
{
    public class UsersService(TravelTipsBasicContext basicContext) : IUsersService
    {
        public UserViewModel? GetUserById(int id)
        {
            var user = basicContext.Users.Find(id);

            return (UserViewModel)user;
        }

        public async Task<UserViewModel> GetUserByUserId(string userId)
        {
            var user = basicContext.Users.FirstOrDefault(user => user.UserId == userId);

            UserViewModel userViewModel;
            if (user == null) 
            { 
                userViewModel = await PostNewUserAsync(userId);
            }
            else
            {
                userViewModel = (UserViewModel)user;
            }

            return userViewModel;
        }

        public async Task<UserViewModel> PostNewUserAsync(string userId)
        {
            var userPostViewModel = new UserPostViewModel { UserId = userId };
            var newUser = userPostViewModel.ToUser();

            await basicContext.Users.AddAsync(newUser);
            await basicContext.SaveChangesAsync();

            return (UserViewModel)newUser;
        }

        public async Task<UserViewModel> UpdateUserAsync(int id, UserPatchViewModel userPatchViewModel)
        {
            var user = basicContext.Users.Find(id) ?? throw UserIdNotFoundException(id);
            user.Email = userPatchViewModel.Email ?? user.Email;
            user.Username = userPatchViewModel.Username ?? user.Username;
            await basicContext.SaveChangesAsync();

            return (UserViewModel)user;
        }
        
        private static Exception UserIdNotFoundException(int id)
        {
            return new Exception($"User not found with id {id}");
        }
    }
}
