using System.Threading.Tasks;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Users
    /// </summary>
    /// <param name="context">context</param>
    public class UsersService(TravelTipsContext context) : IUsersService
    {
        /// <summary>
        /// Get the user by its id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>the user with the id, return null if not found</returns>
        public UserViewModel? GetUserById(int id)
        {
            var user = context.Users.Find(id);

            return (UserViewModel)user;
        }

        /// <summary>
        /// Get the user by its auth0 id
        /// </summary>
        /// <param name="userId">auth0 id</param>
        /// <returns>the user with the auth0 id</returns>
        public async Task<UserViewModel> GetUserByUserId(string userId)
        {
            var user = context.Users.FirstOrDefault(user => user.UserId == userId);

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

        /// <summary>
        /// Create a new user by its auth0 id
        /// </summary>
        /// <param name="userId">auth0 id</param>
        /// <returns>the new user with the auth0 id</returns>
        public async Task<UserViewModel> PostNewUserAsync(string userId)
        {
            var userPostViewModel = new UserPostViewModel { UserId = userId };
            var newUser = userPostViewModel.ToUser();

            await context.Users.AddAsync(newUser);
            await context.SaveChangesAsync();

            return (UserViewModel)newUser;
        }

        /// <summary>
        /// Update a user by its id
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="userPatchViewModel">user information to update</param>
        /// <returns>the update user with rhe id</returns>
        public async Task<UserViewModel> UpdateUserAsync(int id, UserPatchViewModel userPatchViewModel)
        {
            var user = context.Users.Find(id) ?? throw UserIdNotFoundException(id);
            user.Email = userPatchViewModel.Email ?? user.Email;
            user.Username = userPatchViewModel.Username ?? user.Username;
            await context.SaveChangesAsync();

            return (UserViewModel)user;
        }
        
        /// <summary>
        /// Get the exception of user id not found
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>an exception</returns>
        private static Exception UserIdNotFoundException(int id)
        {
            return new Exception($"User not found with id {id}");
        }
    }
}
