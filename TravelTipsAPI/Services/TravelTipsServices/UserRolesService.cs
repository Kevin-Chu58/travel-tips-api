using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of User Roles
    /// </summary>
    /// <param name="context">context</param>
    public class UserRolesService(
        TravelTipsContext context,
        ISubscriptionsService subscriptionsService
    ) : IUserRolesService
    {
        /// <summary>
        /// Check if the user is admin
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>whether is admin</returns>
        public bool IsAdmin(int userId)
        {
            var isAdmin = context.Admins.Find(userId);
            return isAdmin != null;
        }

        /// <summary>
        /// Check if the user is writer
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>whether is writer</returns>
        public bool IsWriter(int userId)
        {
            var isWriter = context.Writers.Find(userId);
            return isWriter != null;
        }

        /// <summary>
        /// Check if the user is banner man
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>whether is banner man</returns>
        public bool IsBannerMan(int userId)
        {
            var isBannerMan = context.BannerMen.Find(userId);
            return isBannerMan != null;
        }

        /// <summary>
        /// Check if the usre is a reviewer
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>whether is reviewer</returns>
        public bool IsReviewer(int userId)
        {
            var isReviewer = context.Reviewers.Find(userId);
            return isReviewer != null;
        }

        // subscriptions

        /// <summary>
        /// Check if the user is a subscribed active member
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>whether is active member</returns>
        public bool IsUserMember(int userId)
        {
            var now = DateTimeOffset.UtcNow;

            var latestSub = subscriptionsService.FindLastSubscriptionByUserId(userId);

            if (latestSub == null || !latestSub.End.HasValue)
                return false;

            // Access is granted if:
            // 1. Status is 'active' or 'past_due' (Stripe's way of saying "trying to pay")
            // 2. AND we haven't passed the End date + 3-day grace buffer
            bool isStatusValid = latestSub.Status == "active" || latestSub.Status == "past_due";
            bool isWithinGracePeriod = now <= latestSub.End.Value.AddDays(3);

            return isStatusValid && isWithinGracePeriod;
        }
    }
}
