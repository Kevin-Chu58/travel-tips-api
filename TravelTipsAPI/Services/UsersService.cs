using TravelTipsAPI.Models.Basic;
using TravelTipsAPI.ViewModels;

namespace TravelTipsAPI.Services
{
    public class UsersService(TravelTipsBasicContext basicContext) : IUsersService
    {
        public UserViewModel? GetUserById(int id)
        {
            var user = basicContext.Users.Find(id);

            return (UserViewModel) user;
        }

        public UserViewModel? GetUserByUserId(string userId)
        {
            var user = basicContext.Users.FirstOrDefault(user => user.UserId == userId);

            return (UserViewModel)user;
        }

        public async Task<UserViewModel?> PostNewUserAsync(string userId)
        {
            var userPostViewModel = new UserPostViewModel { UserId = userId };
            var userToPost = userPostViewModel.ToUser();

            await basicContext.Users.AddAsync(userToPost);
            await basicContext.SaveChangesAsync();

            var updatedUser = GetUserById(userToPost.Id);
            return updatedUser;
        }

        public async Task<UserViewModel?> UpdateUserAsync(UserPatchViewModel user)
        {
            var userToUpdate = basicContext.Users.Find(user.Id);
            userToUpdate.Email = user.Email;
            userToUpdate.Username = user.Username;
            await basicContext.SaveChangesAsync();

            var updatedUser = GetUserById(userToUpdate.Id);
            return updatedUser;
        }

        public bool DoesCurrentUserExist(string userId)
        {
            var currentUser = GetUserByUserId(userId);
            return currentUser != null;
        }
    }
}
