using Microsoft.EntityFrameworkCore;
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
    public class UsersService(IDbContextFactory<TravelTipsContext> contextFactory) : IUsersService
    {
        private readonly TravelTipsContext context = contextFactory.CreateDbContext();

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
        /// Get a list of users by their ids
        /// </summary>
        /// <param name="ids">user ids</param>
        /// <returns>a list of users</returns>
        public IEnumerable<User> GetUsersByIds(IEnumerable<int> ids)
        {
            var users = context.Users.Where(user => ids.Contains(user.Id)).ToList();
            return users;
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
            var user = context.Users.Find(id) ?? throw new Exception(Messages.UserNotFound);

            user.Email = userPatchViewModel.Email ?? user.Email;
            user.Username = userPatchViewModel.Username ?? user.Username;

            await context.SaveChangesAsync();

            return (UserViewModel)user;
        }

        /// <summary>
        /// Update a user agreement status when accepts it
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>updated user agreement status</returns>
        public async Task<bool> AcceptUserAgreementAsync(int id)
        {
            var user = context.Users.Find(id) ?? throw new Exception(Messages.UserNotFound);
            user.UserAgreement = true;
            await context.SaveChangesAsync();

            return user.UserAgreement;
        }
    }
}
