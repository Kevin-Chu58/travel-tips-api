using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_image;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of Users
    /// </summary>
    /// <param name="context">context</param>
    public class UsersService(
        TravelTipsContext context,
        IFollowersService followersService,
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
        /// Get a lsit of users by username with cursor
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="cursor">cursor</param>
        /// <param name="limit">limit</param>
        /// <returns>a list of users</returns>
        public IEnumerable<User> GetUsersByUsernameWithCursor(
            out int? lastUserId,
            string username,
            GeneralCursor? cursor = null,
            int? limit = null
        )
        {
            var query = context.Users.AsQueryable();

            query = query.Where(user => user.Username == username);

            if (cursor != null)
            {
                query = query.Where(user => user.Id > cursor.Id);
            }

            query = query.OrderBy(user => user.Id);

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }
            var users = query.ToList();

            lastUserId = users.LastOrDefault()?.Id;

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

        public User? GetUserByStripeCustomerId(string stripeCustomerId)
        {
            var user = context.Users.FirstOrDefault(user =>
                user.StripeCustomerId == stripeCustomerId
            );
            return user;
        }

        /// <summary>
        /// Get a list of user simple view models
        /// </summary>
        /// <param name="users">users</param>
        /// <param name="showPicture">whether to show picture</param>
        /// <returns>a list of user simple view models</returns>
        public async Task<IEnumerable<UserSimpleViewModel>> GetUserSimpleViewModels(
            IEnumerable<User> users,
            bool showPicture = true
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
                showPicture && imageIds.Count != 0
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
                    Picture = showPicture ? (image?.Url ?? user.ExternalImageUrl) : null,
                };
            });
        }

        /// <summary>
        /// Get a user view model by id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>the user view model with the id</returns>
        public async Task<UserViewModel> GetUserViewModelById(int id)
        {
            var user = await context
                .Users.Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.UserAgreement,
                    u.EmailVerified,
                    IsAdmin = u.Admin != null,
                    IsWriter = u.Writer != null,
                    IsBannerMan = u.BannerMan != null,
                    IsReviewer = u.Reviewer != null,
                    u.ImageId,
                    u.ExternalImageUrl,
                    u.RenewSubscription,
                    u.StripeCustomerId,
                    u.UserSubExtend,
                })
                .SingleAsync();

            // Fetch image in one call (important for performance)
            var image =
                user.ImageId != null
                    ? (await imagesService.GetImagesByIds([user.ImageId.Value])).FirstOrDefault()
                    : null;

            var picture = image?.Url ?? user.ExternalImageUrl;

            return new UserViewModel
            {
                Id = user.Id,
                UserId = user.UserId,
                Username = user.Username ?? "",
                Picture = image?.Url ?? user.ExternalImageUrl,
                Email = user.Email,
                UserAgreement = user.UserAgreement,
                EmailVerified = user.EmailVerified,
                IsAdmin = user.IsAdmin,
                IsWriter = user.IsWriter,
                IsBannerMan = user.IsBannerMan,
                IsReviewer = user.IsReviewer,
                RenewSubscription = user.RenewSubscription,
                StripeCustomerId = user.StripeCustomerId,
                UserSubExtend = (UserSubExtendViewModel)user.UserSubExtend,
            };
        }

        /// <summary>
        /// Update a user by its id
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="userPatchViewModel">user information to update</param>
        /// <returns>the update user with rhe id</returns>
        public async Task<UserViewModel?> UpdateUserAsync(
            int id,
            UserPatchViewModel userPatchViewModel,
            bool returnViewModel = true
        )
        {
            var user = context.Users.Find(id) ?? throw new Exception(Messages.UserNotFound);

            user.Email = userPatchViewModel.Email ?? user.Email;
            user.Username = userPatchViewModel.Username ?? user.Username;

            // stripe settings
            user.RenewSubscription = userPatchViewModel.RenewSubscription ?? user.RenewSubscription;
            user.StripeCustomerId = userPatchViewModel.StripeCustomerId ?? user.StripeCustomerId;
            user.StripeCurrency = userPatchViewModel.StripeCurrency ?? user.StripeCurrency;

            await context.SaveChangesAsync();

            return returnViewModel ? await GetUserViewModelById(id) : null;
        }

        /// <summary>
        /// Remove the user stripe customer id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns></returns>
        public async Task RemoveUserStripeCustomerId(int id)
        {
            var user = context.Users.Find(id) ?? throw new Exception(Messages.UserNotFound);

            user.StripeCustomerId = null;
            await context.SaveChangesAsync();
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
        /// <param name="auth0Id">user auth0 id</param>
        /// <returns>the user profile</returns>
        public async Task<UserProfileViewModel> GetUserProfileViewModel(string auth0Id)
        {
            var user = await context
                .Users.Where(u => u.UserId == auth0Id)
                .Select(u => new
                {
                    u.Id,
                    u.UserId,
                    u.Username,
                    IsAdmin = u.Admin != null,
                    IsWriter = u.Writer != null,
                    IsBannerMan = u.BannerMan != null,
                    NumTrips = u
                        .Trips.Where(t => t.IsPublic == true && t.IsHidden == false)
                        .Count(),
                    NumBookmarks = u
                        .Trips.Where(t => t.IsPublic == true && t.IsHidden == false)
                        .Sum(t => t.BookmarkCount),
                    u.ImageId,
                    u.ExternalImageUrl,
                    FollowerCount = u.FollowerCount >= 0 ? u.FollowerCount : 0,
                    FollowingCount = u.FollowingCount >= 0 ? u.FollowingCount : 0,
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
                Picture = pictureUrl,
                FollowerCount = user.FollowerCount,
                FollowingCount = user.FollowingCount,
                NumTrips = user.NumTrips,
                NumBookmarks = user.NumBookmarks,
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

        // user follower

        public async Task FollowAsync(int followedId, int followingId)
        {
            using var tx = await context.Database.BeginTransactionAsync();

            await followersService.FollowUserAsync(followedId, followingId);
            await UpdateFollowerCountAsync(followedId, followingId, true);

            await tx.CommitAsync();
        }

        public async Task UnfollowAsync(int followedId, int followingId)
        {
            using var tx = await context.Database.BeginTransactionAsync();

            await followersService.UnfollowUserAsync(followedId, followingId);
            await UpdateFollowerCountAsync(followedId, followingId, false);

            await tx.CommitAsync();
        }

        /// <summary>
        /// Update the follower count on user
        /// </summary>
        /// <param name="followedId">follower use id</param>
        /// <param name="followingId">following user id</param>
        /// <param name="increment">whether is increase or decrease</param>
        /// <returns></returns>
        private async Task UpdateFollowerCountAsync(int followedId, int followingId, bool increment)
        {
            var delta = increment ? 1 : -1;

            await context.Database.ExecuteSqlRawAsync(
                "UPDATE db_basic.Users SET FollowingCount = FollowingCount + @delta WHERE Id = @followingId",
                new SqlParameter("@delta", delta),
                new SqlParameter("@followingId", followingId)
            );

            await context.Database.ExecuteSqlRawAsync(
                "UPDATE db_basic.Users SET FollowerCount = FollowerCount + @delta WHERE Id = @followedId",
                new SqlParameter("@delta", delta),
                new SqlParameter("@followedId", followedId)
            );
        }
    }
}
