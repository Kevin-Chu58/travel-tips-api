using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Services.TravelTipsServices.Feed
{
    public class AdsService(TravelTipsContext context) : IAdsService
    {
        /// <summary>
        /// Find an ad by id
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <returns>the id with the id</returns>
        public Ad? FindAdById(int adId)
        {
            return context.Ads.FirstOrDefault(a => a.Id == adId);
        }

        public AdViewModel GetAdById(int adId)
        {
            var adInfo = context
                .Ads.Where(a => a.Id == adId)
                .Select(a => new { Ad = (AdViewModel)a, a.Business })
                .FirstOrDefault();

            var result = adInfo?.Ad;

            if (result == null)
                throw new Exception(Messages.AdNotFound);

            result.BusinessName = adInfo?.Business.Name;

            return result;
        }

        /// <summary>
        /// Get a list of ads by params
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="businessId">business id</param>
        /// <param name="status">ad status</param>
        /// <returns></returns>
        public IEnumerable<AdViewModel> GetAdsByParams(
            int? userId = null,
            int? businessId = null,
            AdEnum.AdStatus? status = null
        )
        {
            if (userId == null && businessId == null && status == null)
                return [];

            var query = context.Ads.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(ad => ad.CreatedBy == userId);
            }

            if (businessId.HasValue)
            {
                query = query.Where(ad => ad.BusinessId == businessId);
            }

            if (status.HasValue)
            {
                var statusStr = GetAdStatusStr(status);
                query = query.Where(ad => ad.Status == statusStr);
            }

            return query.Select(ad => (AdViewModel)ad).ToList();
        }

        /// <summary>
        /// Get a list of ad ids by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of ad ids</returns>
        public IEnumerable<int> GetMyAds(int userId)
        {
            return context.Ads.Where(ad => ad.CreatedBy == userId).Select(ad => ad.Id).ToList();
        }

        /// <summary>
        /// Get a random ad based on target type and target value,
        /// the ad with higher weight has higher chance to be returned.
        /// Only ads with active status and sub status will be returned.
        /// </summary>
        /// <param name="targetType">target type</param>
        /// <param name="targetValue">target value</param>
        /// <returns>ad target, null if not found</returns>
        public Ad? GetAdFeed(List<(string TargetType, string TargetValue)> targets)
        {
            var randomThreshold = Random.Shared.NextDouble();

            // Build the WHERE conditions for each pair
            var conditions = targets.Select(
                (t, i) =>
                    $@"(t.TargetType = {{{i * 2}}} AND (
                        ({{{i * 2}}} = 'keyword' AND t.TargetValue LIKE {{{i * 2 + 1}}} + '%')
                        OR
                        ({{{i * 2}}} != 'keyword' AND t.TargetValue = {{{i * 2 + 1}}})
                    ))"
            );

            var whereClause = string.Join(" OR ", conditions);

            // Flatten params: [type0, value0, type1, value1, ..., randomThreshold]
            var parameters = targets
                .SelectMany(t => new object[] { t.TargetType, t.TargetValue })
                .Append(randomThreshold)
                .ToArray();

            var thresholdIndex = targets.Count * 2;

            var sql =
                $@"
                WITH TopTargets AS (
                    SELECT TOP 1000 t.AdId, SUM(t.Weight) AS Weight
                    FROM db_feed.AdTargets t
                    INNER JOIN db_feed.Ads a ON t.AdId = a.Id
                    WHERE a.SubStatus = 'active' AND a.Status = 'active'
                      AND ({whereClause})
                    GROUP BY t.AdId
                    ORDER BY Weight DESC
                ),
                WeightedPool AS (
                    SELECT AdId,
                           SUM(Weight) OVER() AS TotalWeight,
                           SUM(Weight) OVER(ORDER BY AdId) / CAST(NULLIF(SUM(Weight) OVER(), 0) AS FLOAT) AS CumulativeWeight
                    FROM TopTargets
                )
                SELECT TOP 1 a.*
                FROM db_feed.Ads a
                INNER JOIN WeightedPool w ON a.Id = w.AdId
                WHERE w.CumulativeWeight >= {{{thresholdIndex}}}
                ORDER BY w.CumulativeWeight ASC";

            return context.Ads.FromSqlRaw(sql, parameters).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Create a new ad
        /// </summary>
        /// <param name="postViewModel">new ad</param>
        /// <param name="userId">user id</param>
        /// <param name="businessId">business id</param>
        /// <returns>the newly created ad</returns>
        public async Task<AdViewModel> PostNewAd(
            AdPostViewModel postViewModel,
            int userId,
            int businessId
        )
        {
            if (postViewModel.ImageId == null)
                throw new Exception(Messages.AdImageIsMissing);

            var ad = new Ad
            {
                CreatedBy = userId,
                BusinessId = businessId,
                ImageId = (int)postViewModel.ImageId,
                Title = postViewModel.Title,
                Text = postViewModel.Text,
                LinkLabel = postViewModel.ButtonLabel,
                Link = postViewModel.Link,
                TemplateId = postViewModel.TemplateId,
                Status = GetAdStatusStr(AdStatus.Pending)!,
                RenewSub = true,
            };

            context.Ads.Add(ad);
            await context.SaveChangesAsync();

            // create a sub log for the new ad creation
            await PostNewAdSubLog(ad.Id, Messages.NewAdCreated, null, null);

            return (AdViewModel)ad;
        }

        /// <summary>
        /// Update an ad details
        /// </summary>
        /// <param name="ad">ad</param>
        /// <param name="adPatch">ad details to be updated</param>
        /// <returns>the updated ad</returns>
        public async Task<AdViewModel> UpdateAd(Ad ad, AdPatchViewModel adPatch)
        {
            var tx = context.Database.BeginTransaction();

            // not required to update image id, since the same image is overwritten
            // by the new image file instead of creating a new one

            ad.Title = adPatch.Title ?? ad.Title;
            ad.Text = adPatch.Text ?? ad.Text;
            ad.LinkLabel = adPatch.LinkLabel ?? ad.LinkLabel;
            ad.Link = adPatch.Link ?? ad.Link;
            ad.TemplateId = adPatch.TemplateId ?? ad.TemplateId;

            await context.SaveChangesAsync();

            // create a sub log for the ad update
            await PostNewAdSubLog(ad.Id, Messages.AdUpdated, null, null);

            tx.Commit();

            return (AdViewModel)ad;
        }

        /// <summary>
        /// Update an ad active status
        /// </summary>
        /// <param name="ad">ad</param>
        /// <param name="isActive">active status</param>
        /// <returns>the new status</returns>
        public async Task<string> UpdateAdActiveStatus(Ad ad, bool isActive)
        {
            // Only update status if the current status is Active or Inactive
            if (
                ad.Status != GetAdStatusStr(AdStatus.Active)
                && ad.Status != GetAdStatusStr(AdStatus.Inactive)
            )
            {
                throw new Exception(Messages.AdStatusCannotBeUpdated);
            }

            var tx = context.Database.BeginTransaction();

            ad.Status = isActive
                ? GetAdStatusStr(AdStatus.Active)!
                : GetAdStatusStr(AdStatus.Inactive)!;

            await context.SaveChangesAsync();

            // create a sub log for the ad active status update
            var message = string.Format(
                isActive ? Messages.AdActive : Messages.AdInactive,
                ad.Title
            );
            await PostNewAdSubLog(ad.Id, message, null, null);

            tx.Commit();

            return ad.Status;
        }

        /// <summary>
        /// Update an ad status
        /// </summary>
        /// <param name="ad">ad</param>
        /// <param name="status">ad status</param>
        /// <param name="reason">reason for the change</param>
        /// <returns>the new status</returns>
        public async Task<string> UpdateAdStatus(Ad ad, AdStatus status, string? reason = null)
        {
            var statusStr = GetAdStatusStr(status);
            if (statusStr == null)
                throw new Exception(Messages.AdStatusInvalid);

            var tx = context.Database.BeginTransaction();

            ad.Status = statusStr;
            await context.SaveChangesAsync();

            string? messageType = null;
            switch (status)
            {
                case AdStatus.Active:
                    messageType = Messages.AdActive;
                    break;
                case AdStatus.Inactive:
                    messageType = Messages.AdInactive;
                    break;
                case AdStatus.RequestChange:
                    messageType = string.Format(Messages.AdRequestChange, reason);
                    break;
                case AdStatus.Denied:
                    messageType = string.Format(Messages.AdDenied, reason);
                    break;
                default:
                    break;
            }

            // create a sub log for the ad status update
            if (messageType != null)
            {
                var message = string.Format(messageType, ad.Title);
                await PostNewAdSubLog(ad.Id, message, null, null);
            }

            tx.Commit();

            return ad.Status;
        }

        /// <summary>
        /// Update an ad stripe sub id
        /// </summary>
        /// <param name="ad">ad id</param>
        /// <param name="stripeSubId">stripe subscription id</param>
        /// <pram name="stripeItemId">stripe item id</param>
        /// <returns></returns>
        public async Task UpdateAdStripeSubInfo(Ad ad, string stripeSubId, string stripeItemId)
        {
            ad.StripeSubscriptionId = stripeSubId;
            ad.StripeItemId = stripeItemId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Update an ad Stripe subscription status,
        /// which is reflected from the Stripe webhook events
        /// </summary>
        /// <param name="ad">ad id</param>
        /// <param name="subStatus">new sub status</param>
        /// <returns></returns>
        public async Task UpdateAdStripeSubStatus(Ad ad, string subStatus)
        {
            ad.SubStatus = subStatus;
            await context.SaveChangesAsync();
        }

        // sub ad logs

        /// <summary>
        /// Get a list of ad sub logs by ad id
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <param name="cursor">general cursor for pagination</param>
        /// <param name="limit">limit for pagination</param>
        /// <returns>a list of ad sub logs under the ad</returns>
        public IEnumerable<AdSubLogViewModel> GetAdSubLogsByAdIdWithCursor(
            int adId,
            GeneralCursor? cursor = null,
            int? limit = null
        )
        {
            var query = context.AdSubLogs.AsQueryable().Where(log => log.AdId == adId);

            if (cursor != null)
            {
                query = query.Where(log => log.Id < cursor.Id);
            }

            query = query.OrderByDescending(log => log.Id);

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }

            return query.Select(log => (AdSubLogViewModel)log).ToList();
        }

        /// <summary>
        /// Create new ad sub log under an ad
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <param name="note">note to the sub log</param>
        /// <param name="oldValue">old weight, only if there is a change</param>
        /// <param name="newValue">new weight, only if there is a change</param>
        /// <returns></returns>
        public async Task PostNewAdSubLog(int adId, string note, int? oldValue, int? newValue)
        {
            var adSubLog = new AdSubLog
            {
                AdId = adId,
                Time = DateTimeOffset.UtcNow,
                Note = note,
                OldValue = oldValue,
                NewValue = newValue,
            };
            context.AdSubLogs.Add(adSubLog);
            await context.SaveChangesAsync();
        }

        // subscription status

        /// <summary>
        /// Update ad subscription renewal status
        /// </summary>
        /// <param name="ad">ad</param>
        /// <param name="renewSub">ad renew sub status</param>
        /// <returns></returns>
        public async Task UpdateAdSubscriptionRenewal(Ad ad, bool renewSub)
        {
            ad.RenewSub = renewSub;
            await context.SaveChangesAsync();
        }
    }
}
