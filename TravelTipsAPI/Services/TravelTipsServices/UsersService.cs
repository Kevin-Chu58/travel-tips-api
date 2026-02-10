using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_image;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of Users
    /// </summary>
    /// <param name="context">context</param>
    public class UsersService(
        TravelTipsContext context,
        IUserRolesService userRolesService,
        IImagesService imagesService
    ) : IUsersService
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
        /// Get a list of user simple view models
        /// </summary>
        /// <param name="users">users</param>
        /// <returns>a list of user simple view models</returns>
        public async Task<IEnumerable<UserSimpleViewModel>> GetUserSimpleViewModels(
            IEnumerable<User> users
        )
        {
            var userList = users.ToList();

            // Collect all imageIds we need
            var imageIds = userList
                .Where(u => u.ImageId != null)
                .Select(u => u.ImageId!.Value)
                .Distinct()
                .ToList();

            // Fetch images in one call (important for performance)
            var images =
                imageIds.Count != 0
                    ? (await imagesService.GetImagesByIds([.. imageIds])).ToDictionary(i => i.Id)
                    : [];

            return userList.Select(user =>
            {
                images.TryGetValue(user.ImageId ?? -1, out var image);

                return new UserSimpleViewModel
                {
                    Id = user.Id,
                    UserId = user.UserId,
                    Username = user.Username ?? "",
                    Picture = image?.Url ?? user.ExternalImageUrl,
                };
            });
        }

        /// <summary>
        /// Get a list of user view models
        /// </summary>
        /// <param name="users">users</param>
        /// <returns>a list of user view models</returns>
        public async Task<IEnumerable<UserViewModel>> GetUserViewModels(IEnumerable<User> users)
        {
            var userList = users.ToList();

            // Collect all imageIds we need
            var imageIds = userList
                .Where(u => u.ImageId != null)
                .Select(u => u.ImageId!.Value)
                .Distinct()
                .ToList();

            // Fetch images in one call (important for performance)
            var images =
                imageIds.Count != 0
                    ? (await imagesService.GetImagesByIds([.. imageIds])).ToDictionary(i => i.Id)
                    : [];

            return userList.Select(user =>
            {
                images.TryGetValue(user.ImageId ?? -1, out var image);

                return new UserViewModel
                {
                    Id = user.Id,
                    UserId = user.UserId,
                    Username = user.Username ?? "",
                    Picture = image?.Url ?? user.ExternalImageUrl,
                    Email = user.Email,
                    UserAgreement = user.UserAgreement,
                    IsAdmin = userRolesService.IsAdmin(user.Id),
                    IsWriter = userRolesService.IsWriter(user.Id),
                };
            });
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

        // user profile

        /// <summary>
        /// Get the user profile by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>the user profile</returns>
        public async Task<UserProfileViewModel> GetUserProfileViewModel(int id)
        {
            var user = await context
                .Users.Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.Username,
                    IsAdmin = u.Admin != null,
                    IsWriter = u.Writer != null,
                    NumTrips = u
                        .Trips.Where(t => t.IsPublic == true && t.IsHidden == false)
                        .Count(),
                    NumBookmarks = u
                        .Trips.Where(t => t.IsPublic == true && t.IsHidden == false)
                        .Sum(t => t.BookmarkCount),
                    u.ImageId,
                    u.ExternalImageUrl,
                })
                .SingleAsync();

            if (user is null)
                throw new Exception(Messages.UserNotFound);

            string? pictureUrl = user.ExternalImageUrl;

            if (user.ImageId != null)
            {
                var images = await imagesService.GetImagesByIds([user.ImageId.Value]);
                pictureUrl = images.FirstOrDefault()?.Url;
            }

            var userProfile = new UserProfileViewModel
            {
                Id = user.Id,
                UserId = user.UserId,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                IsWriter = user.IsWriter,
                NumTrips = user.NumTrips,
                NumBookmarks = user.NumBookmarks,
                Picture = pictureUrl,
            };

            return userProfile;
        }

        // user picture

        /// <summary>
        /// Update user picture with an existing image
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="image">image view model</param>
        /// <returns>the updated picture url</returns>
        public async Task<string?> UpdateUserPicture(User user, ImageViewModel? image)
        {
            user.ImageId = image?.Id ?? null;

            await context.SaveChangesAsync();

            if (image?.Id is null)
                return user.ExternalImageUrl;

            return image.Url;
        }
    }
}
