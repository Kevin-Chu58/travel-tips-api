using Stripe;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Services.StripeServices.StripeSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Feed
{
    public class AdsService(TravelTipsContext context, IStripeService stripeService) : IAdsService
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
        /// <returns></returns>
        public async Task UpdateAdStripeSubId(Ad ad, string stripeSubId)
        {
            ad.StripeSubscriptionId = stripeSubId;
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
        /// <returns>a list of ad sub logs under the ad</returns>
        public IEnumerable<AdSubLogViewModel> GetAdSubLogsByAdId(int adId)
        {
            return context
                .AdSubLogs.Where(log => log.AdId == adId)
                .OrderByDescending(log => log.Time)
                .Select(log => (AdSubLogViewModel)log)
                .ToList();
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
        /// Update the subscription status (auto-renew or not) in Stripe
        /// </summary>
        /// <param name="subId">subscription id</param>
        /// <param name="cancelSub">cancel subscription status</param>
        /// <returns></returns>
        public async Task UpdateAdSubscriptionStatus(string subId, bool cancelSub)
        {
            var service = new SubscriptionService();
            var serviceOptions = stripeService.GetRequestOptions();
            var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = cancelSub };

            await service.UpdateAsync(subId, options, serviceOptions);
        }
    }
}
