using System.Threading.Tasks;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
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
        public User GetUserById(int id)
        {
            var user = context.Users.Find(id);

            if (user == null)
                throw new Exception(Messages.UserNotFound);

            return user;
        }

        /// <summary>
        /// Get the user by its auth0 id
        /// </summary>
        /// <param name="userId">auth0 id</param>
        /// <returns>the user with the auth0 id</returns>
        public User? GetUserByUserId(string userId)
        {
            var user = context.Users.FirstOrDefault(user => user.UserId == userId);

            return user;
        }

        /// <summary>
        /// Create a new user by its auth0 id
        /// </summary>
        /// <param name="userId">auth0 id</param>
        /// <returns>the new user with the auth0 id</returns>
        public async Task<UserViewModel> PostNewUserAsync(UserPostViewModel userPost)
        {
            var newUser = userPost.ToUser();

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
        public async Task<UserViewModel> UpdateUserAsync(
            int id,
            UserPatchViewModel userPatchViewModel
        )
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
