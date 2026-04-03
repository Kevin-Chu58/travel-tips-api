using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

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
            var tx = context.Database.BeginTransaction();

            var ad = new Ad
            {
                CreatedBy = userId,
                BusinessId = businessId,
                ImageId = postViewModel.ImageId,
                Title = postViewModel.Title,
                Text = postViewModel.Text,
                ButtonLabel = postViewModel.ButtonLabel,
                Link = postViewModel.Link,
                Status = GetAdStatusStr(AdStatus.Pending)!,
            };

            context.Ads.Add(ad);
            await context.SaveChangesAsync();

            // create a sub log for the new ad creation
            var message = string.Format(Messages.NewAdCreated, ad.Title);
            await PostNewAdSubLog(ad.Id, message, null, null);

            tx.Commit();

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

            ad.ImageId = adPatch.ImageId ?? ad.ImageId;
            ad.Title = adPatch.Title ?? ad.Title;
            ad.Text = adPatch.Text ?? ad.Text;
            ad.ButtonLabel = adPatch.ButtonLabel ?? ad.ButtonLabel;

            await context.SaveChangesAsync();

            // create a sub log for the ad update
            var message = string.Format(Messages.AdUpdated, ad.Title);
            await PostNewAdSubLog(ad.Id, message, null, null);

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
                || ad.Status != GetAdStatusStr(AdStatus.Inactive)
            )
            {
                throw new Exception(Messages.AdStatusCannotBeUpdated);
            }

            if (ad.Status == GetAdStatusStr(AdStatus.Active) && isActive)
            {
                // If the ad is already active and the new status is active, do nothing
                return ad.Status;
            }

            if (ad.Status == GetAdStatusStr(AdStatus.Inactive) && !isActive)
            {
                // If the ad is already inactive and the new status is inactive, do nothing
                return ad.Status;
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
        /// <returns>the new status</returns>
        public async Task<string> UpdateAdStatus(Ad ad, AdStatus status)
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
                    messageType = Messages.AdRequestChange;
                    break;
                case AdStatus.Denied:
                    messageType = Messages.AdDenied;
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
                .Select(log => (AdSubLogViewModel)log)
                .OrderByDescending(log => log.Time)
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
    }
}
