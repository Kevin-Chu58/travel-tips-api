using TravelTipsAPI.BackgroundServices;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Services.TravelTipsServices.Plan;
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
        /// <param name="id">user id</param>
        /// <returns>whether is admin</returns>
        public bool IsAdmin(int id)
        {
            var isAdmin = context.Admins.Find(id);

            return isAdmin != null;
        }

        /// <summary>
        /// Check if the user is writer
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>whether is writer</returns>
        public bool IsWriter(int id)
        {
            var isWriter = context.Writers.Find(id);

            return isWriter != null;
        }

        /// <summary>
        /// Check if the user is banner man
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>whether is banner man</returns>
        public bool IsBannerMan(int id)
        {
            var isBannerMan = context.BannerMen.Find(id);

            return isBannerMan != null;
        }

        // subscriptions

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
