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

            var newUserViewModel = GetUserById(newUser.Id);
            return newUserViewModel;
        }

        public async Task<UserViewModel> UpdateUserAsync(UserPatchViewModel user)
        {
            var userToUpdate = basicContext.Users.Find(user.Id);
            userToUpdate.Email = user.Email;
            userToUpdate.Username = user.Username;
            await basicContext.SaveChangesAsync();

            var updatedUser = GetUserById(userToUpdate.Id);
            return updatedUser;
        }
    }
}
