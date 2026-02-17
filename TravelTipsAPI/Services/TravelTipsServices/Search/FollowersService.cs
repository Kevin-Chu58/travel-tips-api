using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Services.TravelTipsServices.Search
{
    public class FollowersService(TravelTipsContext context) : IFollowersService
    {
        /// <summary>
        /// Get a list of users that the user followed
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="cursor">cursor</param>
        /// <param name="limit">limit</param>
        /// <returns>a list of users</returns>
        public IEnumerable<User> GetFollowedUsersByUserIdWithCursor(
            int userId,
            out int? followerId,
            GeneralCursor? cursor = null,
            int? limit = null
        )
        {
            var query = context.Followers.AsQueryable();

            query = query.Where(f => f.Following == userId);

            if (cursor != null)
            {
                query = query.Where(f => f.Id > cursor.Id);
            }

            query = query.OrderBy(f => f.Id);

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }

            var result = query
                .Include(f => f.FollowedNavigation)
                .Select(f => new { Followed = f.FollowedNavigation, FollowedId = f.Id })
                .ToList();

            followerId = result.LastOrDefault()?.FollowedId;

            var followedUsers = result.Select(f => f.Followed).ToList();
            return followedUsers;
        }

        /// <summary>
        /// Get a list of users that the user is following
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="cursor">cursor</param>
        /// <param name="limit">limit</param>
        /// <returns>a list of users</returns>
        public IEnumerable<User> GetFollowingUsersByUserIdWithCursor(
            int userId,
            out int? followerId,
            GeneralCursor? cursor = null,
            int? limit = null
        )
        {
            var query = context.Followers.AsQueryable();

            query = query.Where(f => f.Followed == userId);

            if (cursor != null)
            {
                query = query.Where(f => f.Id > cursor.Id);
            }

            query = query.OrderBy(f => f.Id);

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }

            var result = query
                .Include(f => f.FollowingNavigation)
                .Select(f => new { Following = f.FollowingNavigation, FollowedId = f.Id })
                .ToList();

            followerId = result.LastOrDefault()?.FollowedId;

            var followingUsers = result.Select(f => f.Following).ToList();
            return followingUsers;
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
