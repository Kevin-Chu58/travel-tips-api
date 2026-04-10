using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdTargetEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Feed
{
    public class AdTargetsService(
        TravelTipsContext context,
        IAdsService adsService,
        ITargetRulesService targetRulesService
    ) : IAdTargetsService
    {
        /// <summary>
        /// Find an ad target with its id
        /// </summary>
        /// <param name="adTargetId">ad target id</param>
        /// <returns>an ad target with the id, if not found return null</returns>
        public Models.TravelTipsModels.AdTarget? FindAdTargetById(int adTargetId)
        {
            return context.AdTargets.FirstOrDefault(at => at.Id == adTargetId);
        }

        /// <summary>
        /// Get a list of ad targets by ad id
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <returns>a list of ad targets</returns>
        public IEnumerable<AdTargetViewModel> GetAdTargetsByAdId(int adId)
        {
            return context
                .AdTargets.Where(at => at.AdId == adId)
                .Select(at => (AdTargetViewModel)at)
                .ToList();
        }

        /// <summary>
        /// Create a new ad target
        /// </summary>
        /// <param name="postViewModel">new ad target</param>
        /// <param name="adId">ad id</param>
        /// <returns></returns>
        public async Task PostNewAdTarget(AdTargetPostViewModel postViewModel, int adId)
        {
            var adTargets = GetAdTargetsByAdId(adId);

            // Check if the ad already has the maximum number of ad targets
            if (adTargets.Count() >= Global.AD_TARGET_LIMIT_PER_AD)
            {
                throw new Exception(Messages.AdTargetLimitReached);
            }

            var adTarget = new Models.TravelTipsModels.AdTarget
            {
                AdId = adId,
                TargetType = postViewModel.TargetType,
                TargetValue = postViewModel.TargetValue,
                Weight = postViewModel.Weight,
            };

            var targetRule = targetRulesService.GetTargetRule(
                adTarget.TargetType,
                adTarget.TargetValue
            );

            // Check if the ad target meets the requirement of its target rule
            if (targetRule != null && adTarget.Weight < targetRule.MinWeight)
            {
                throw new Exception(Messages.TargetRuleMinWeightNotMet);
            }

            var tx = context.Database.BeginTransaction();

            // If there is no primary ad target for the ad, set the new ad target as primary
            var primaryAdTarget = adTargets.FirstOrDefault(at => at.IsPrimary);
            if (primaryAdTarget == null)
            {
                adTarget.IsPrimary = true;
            }
            else
            {
                adTarget.IsPrimary = false;
            }

            await context.AdTargets.AddAsync(adTarget);
            await context.SaveChangesAsync();

            // create a sub log for the new ad target creation
            var message = string.Format(
                Messages.NewAdTargetAdded,
                GetAdTargetDescription(adTarget)
            );
            await adsService.PostNewAdSubLog(adId, message, null, null);

            tx.Commit();
        }

        /// <summary>
        /// Add weight to an target's weight
        /// </summary>
        /// <param name="adTarget">ad target</param>
        /// <param name="increment">increment on weight</param>
        /// <returns>the new weight</returns>
        public async Task<int> IncreaseAdTargetWeight(
            Models.TravelTipsModels.AdTarget adTarget,
            int increment
        )
        {
            var targetRule = targetRulesService.GetTargetRule(
                adTarget.TargetType,
                adTarget.TargetValue
            );

            // Check if the ad target meets the requirement of its target rule
            if (targetRule != null && adTarget.Weight + increment < targetRule.MinWeight)
            {
                throw new Exception(Messages.TargetRuleMinWeightNotMet);
            }

            var tx = context.Database.BeginTransaction();

            var weight = adTarget.Weight;
            adTarget.Weight += increment;
            await context.SaveChangesAsync();

            // create a sub log for the ad target weight increase
            var message = string.Format(
                Messages.AdTargetWeightIncreased,
                GetAdTargetDescription(adTarget)
            );
            await adsService.PostNewAdSubLog(adTarget.AdId, message, weight, adTarget.Weight);

            tx.Commit();

            return adTarget.Weight;
        }

        /// <summary>
        /// Set the future weight of an ad target.
        /// The future weight will be applied in the next round of ad serving.
        /// This is to avoid the weight change takes effect immediately and cause instability of ad serving.
        /// </summary>
        /// <param name="adTarget">ad target</param>
        /// <param name="decrement">(future) decrement on weight</param>
        /// <returns>the future new weight</returns>
        public async Task<int> DecreaseAdTargetWeight(
            Models.TravelTipsModels.AdTarget adTarget,
            int decrement
        )
        {
            var targetRule = targetRulesService.GetTargetRule(
                adTarget.TargetType,
                adTarget.TargetValue
            );

            // Check if the ad target meets the requirement of its target rule
            if (targetRule != null && adTarget.Weight - decrement < targetRule.MinWeight)
            {
                throw new Exception(Messages.TargetRuleMinWeightNotMet);
            }

            var tx = context.Database.BeginTransaction();

            adTarget.FutureWeight = Math.Max(0, adTarget.Weight - decrement);
            await context.SaveChangesAsync();

            // create a sub log for the ad target weight decrease
            var message = string.Format(
                Messages.AdTargetWeightDecreased,
                GetAdTargetDescription(adTarget)
            );
            await adsService.PostNewAdSubLog(
                adTarget.AdId,
                message,
                adTarget.Weight,
                adTarget.FutureWeight
            );

            tx.Commit();

            return (int)adTarget.FutureWeight;
        }

        /// <summary>
        /// Set an ad target as the primary ad target of the ad
        /// </summary>
        /// <param name="adTarget">ad target</param>
        /// <returns>the ad target id</returns>
        public async Task<int> SetAdTargetAsPrimary(Models.TravelTipsModels.AdTarget adTarget)
        {
            var adTargets = GetAdTargetsByAdId(adTarget.AdId);
            var primaryAdTarget = adTargets.FirstOrDefault(at => at.IsPrimary);

            if (primaryAdTarget != null)
            {
                primaryAdTarget.IsPrimary = false;
            }

            adTarget.IsPrimary = true;

            await context.SaveChangesAsync();
            return adTarget.Id;
        }

        /// <summary>
        /// Cancel/reinstate an ad target
        /// </summary>
        /// <param name="adTarget">ad target</param>
        /// <param name="cancel">cancel status</param>
        /// <returns></returns>
        public async Task CancelAdTarget(Models.TravelTipsModels.AdTarget adTarget, bool cancel)
        {
            var tx = context.Database.BeginTransaction();

            if (cancel)
            {
                adTarget.FutureWeight = 0;
            }
            else
            {
                adTarget.FutureWeight = null;
            }

            await context.SaveChangesAsync();

            // create a sub log for the ad target weight decrease
            var message = string.Format(
                cancel ? Messages.AdTargetCanceled : Messages.AdTargetReinstated,
                GetAdTargetDescription(adTarget)
            );
            await adsService.PostNewAdSubLog(adTarget.AdId, message, null, null);

            tx.Commit();
        }

        /// <summary>
        /// Update ad targets of an ad to the next cycle
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <returns></returns>
        public async Task UpdateAdTargetCycleByAdId(int adId)
        {
            var adTargets = context.AdTargets.Where(at => at.AdId == adId).ToList();

            var tx = context.Database.BeginTransaction();

            var deletedAdTargets = new List<string>();

            // Apply the future weight to the current weight, and remove the ad target if the future weight is 0
            foreach (var adTarget in adTargets)
            {
                if (adTarget.FutureWeight.HasValue)
                {
                    adTarget.Weight = adTarget.FutureWeight.Value;
                    adTarget.FutureWeight = null;
                }
                if (adTarget.FutureWeight == 0)
                {
                    deletedAdTargets.Add(GetAdTargetDescription(adTarget));
                    context.AdTargets.Remove(adTarget);
                }
            }

            // Set the first ad target as primary if there is no primary ad target after the update
            var firstAdTarget = adTargets.FirstOrDefault();
            if (firstAdTarget != null && !adTargets.Any(at => at.IsPrimary))
            {
                firstAdTarget.IsPrimary = true;
            }

            await context.SaveChangesAsync();

            // create a sub log for the new ad target cycle
            var message = string.Format(
                Messages.AdNewCycle,
                adId,
                string.Join(", ", deletedAdTargets)
            );
            await adsService.PostNewAdSubLog(adId, message, null, null);

            tx.Commit();
        }

        private string GetAdTargetDescription(Models.TravelTipsModels.AdTarget adTarget)
        {
            return $"{adTarget.TargetType} - {adTarget.TargetValue}";
        }
    }
}
