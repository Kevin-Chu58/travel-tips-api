using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_plan;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.PlanSchema;
using static TravelTipsAPI.Utils.ObjectUtils;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Controllers.TravelTips.Plan
{
    [Route("api/[controller]")]
    public class SubscriptionsController(ISubscriptionsService subscriptionsService)
        : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<SearchResults<SubscriptionViewModel>> GetMySubscriptions(
            [FromQuery] string? cursor = null,
            [FromQuery] int? limit = null
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            limit ??= Global.SUBSCRIPTION_DEFAULT_LIMIT;

            // decode cursor if provided
            GeneralCursor? subscriptionCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                subscriptionCursor = DecodeCursor<GeneralCursor>(cursor);
                if (subscriptionCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            var subscriptions = subscriptionsService.GetSubscriptionsByUserIdWithCursor(
                userId,
                subscriptionCursor,
                limit
            );

            // encode cursor
            var subscriptionHistory = subscriptions.ToList();
            string? newCursor = null;
            if (subscriptionHistory.Count == limit)
            {
                var lastSubscription = subscriptionHistory.Last();
                newCursor = EncodeCursor(new GeneralCursor { Id = lastSubscription.Id });
            }

            var result = new SearchResults<SubscriptionViewModel>
            {
                Results = subscriptionHistory,
                Cursor = newCursor,
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("active")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<SubscriptionViewModel?> GetMyActiveSubscription()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var subscription = subscriptionsService.GetActiveSubscriptionByUserId(userId);
            return Ok(subscription);
        }
    }
}
