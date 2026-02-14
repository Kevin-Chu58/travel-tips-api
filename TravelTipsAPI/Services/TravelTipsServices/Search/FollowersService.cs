using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Search
{
    public class FollowersService(TravelTipsContext context) : IFollowersService
    {
        /// <summary>
        /// Get a list of user ids that the user followed
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of user ids</returns>
        public IEnumerable<int> GetFollowedUserIdsByUserId(int userId)
        {
            var followedIds = context
                .Followers.Where(following => following.Following == userId)
                .Select(following => following.Followed)
                .ToList();
            return followedIds;
        }

        /// <summary>
        /// Get a list of user ids that the user is following
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of user ids</returns>
        public IEnumerable<int> GetFollowingUserIdsByUserId(int userId)
        {
            var followingIds = context
                .Followers.Where(following => following.Followed == userId)
                .Select(following => following.Following)
                .ToList();
            return followingIds;
        }

        /// <summary>
        /// Whether a user follows another user
        /// </summary>
        /// <param name="followedId">followed user id</param>
        /// <param name="followingId">following user id</param>
        /// <returns>whether a user follows another user</returns>
        public bool IsFollowing(int followedId, int followingId)
        {
            return context.Followers.Any(f =>
                f.Followed == followedId && f.Following == followingId
            );
        }

        /// <summary>
        /// Follow a user
        /// </summary>
        /// <param name="followedId">followed user id</param>
        /// <param name="followingId">following user id</param>
        /// <returns></returns>
        public async Task FollowUserAsync(int followedId, int followingId)
        {
            if (followedId == followingId)
                throw new Exception(Messages.FollowSelf);

            var follower = await context.Followers.FirstOrDefaultAsync(f =>
                f.Followed == followedId && f.Following == followingId
            );
            if (follower != null)
                throw new Exception(Messages.FollowAlreadyExists);

            follower = new Follower { Followed = followedId, Following = followingId };

            await context.Followers.AddAsync(follower);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Unfollow a user
        /// </summary>
        /// <param name="followedId">followed user id</param>
        /// <param name="followingId">following user id</param>
        /// <returns></returns>
        public async Task UnfollowUserAsync(int followedId, int followingId)
        {
            if (followedId == followingId)
                throw new Exception(Messages.FollowSelf);

            var follower = await context.Followers.FirstOrDefaultAsync(f =>
                f.Followed == followedId && f.Following == followingId
            );

            if (follower == null)
                throw new Exception(Messages.FollowNotFound);

            context.Followers.Remove(follower);
            await context.SaveChangesAsync();
        }
    }
}
