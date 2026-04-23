using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using TravelTipsAPI.ViewModels.Stripe;
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
        public AdTarget? FindAdTargetById(int adTargetId)
        {
            return context.AdTargets.FirstOrDefault(at => at.Id == adTargetId);
        }

        /// <summary>
        /// Find an ad target with its target type and target value. If not found, return null
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <param name="targetType">target type</param>
        /// <param name="targetValue">target value</param>
        /// <returns>an ad target with the id, if not found return null</returns>
        public AdTarget? FindAdTargetByParams(int adId, string targetType, string? targetValue)
        {
            return context.AdTargets.FirstOrDefault(at =>
                at.TargetType == targetType && at.TargetValue == targetValue && at.AdId == adId
            );
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
        /// Get total weights by ad id
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <returns>total weights of the ad</returns>
        public int GetWeightsByAdId(int adId)
        {
            return context.AdTargets.Where(at => at.AdId == adId).Sum(at => at.Weight);
        }

        /// <summary>
        /// Get an ad target analytics of the same type and value
        /// </summary>
        /// <param name="adTarget">ad target</param>
        /// <returns>ad target analytics</returns>
        public AdTargetAnalytics GetAdTargetRanking(AdTarget adTarget)
        {
            // Create the search pattern first to avoid manual string concatenation in SQL
            var keywordPattern = $"{adTarget.TargetValue}%";

            var top1000targets = context
                .Database.SqlQuery<AdTargetAnalyticsForSql>(
                    $@"SELECT TOP 1000 
                        t.Id, 
                        ROW_NUMBER() OVER (
                            ORDER BY t.Weight DESC
                            ) AS Rank,
                        (CAST (
                            t.Weight AS FLOAT) / SUM(t.Weight) OVER()
                            ) * 100 AS [Percent]
                    FROM db_feed.AdTargets t
                    INNER JOIN db_feed.Ads a ON t.AdId = a.Id
                    WHERE a.SubStatus = 'active' AND a.Status = 'active'
                      AND t.TargetType = {adTarget.TargetType}
                      AND (
                          ({adTarget.TargetType} = 'keyword' AND t.TargetValue LIKE {keywordPattern})
                          OR
                          ({adTarget.TargetType} != 'keyword' AND t.TargetValue = {adTarget.TargetValue})
                    )"
                )
                .AsEnumerable()
                .ToList();

            var analytic = top1000targets.FirstOrDefault(t => t.Id == adTarget.Id);

            if (analytic == null)
            {
                return new AdTargetAnalytics
                {
                    Id = adTarget.Id,
                    Rank = "1000+",
                    Percent = 0,
                };
            }

            analytic.Percent = Math.Round(analytic.Percent, 2);
            return (AdTargetAnalytics)analytic;
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

            var adTarget = new AdTarget
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
            adTarget.IsPrimary = primaryAdTarget == null ? true : false;

            await context.AdTargets.AddAsync(adTarget);
            await context.SaveChangesAsync();

            // create a sub log for the new ad target creation
            var message = string.Format(
                Messages.NewAdTargetAdded,
                GetAdTargetDescription(adTarget)
            );
            await adsService.PostNewAdSubLog(adId, message, null, null);

            await tx.CommitAsync();
        }

        public async Task UpdateAdTarget(AdTarget adTarget, StripeAdWeightRequest request)
        {
            var tx = context.Database.BeginTransaction();

            // create a sub log for the ad target weight increase
            if (request.Weight > adTarget.Weight)
            {
                var message = string.Format(
                    Messages.AdTargetWeightIncreased,
                    GetAdTargetDescription(adTarget)
                );
                await adsService.PostNewAdSubLog(
                    adTarget.AdId,
                    message,
                    adTarget.Weight,
                    request.Weight
                );
            }
            else if (request.Weight < adTarget.Weight)
            {
                var message = string.Format(
                    Messages.AdTargetWeightDecreased,
                    GetAdTargetDescription(adTarget)
                );
                await adsService.PostNewAdSubLog(
                    adTarget.AdId,
                    message,
                    adTarget.Weight,
                    request.Weight
                );
            }

            if (
                request.TargetType != adTarget.TargetType
                || request.TargetValue != adTarget.TargetValue
            )
            {
                // create a sub log for the ad target update
                var message = string.Format(
                    Messages.AdTargetTypeValueUpdated,
                    GetAdTargetDescription(adTarget),
                    GetAdTargetDescription(
                        new Models.TravelTipsModels.AdTarget
                        {
                            TargetType = request.TargetType,
                            TargetValue = request.TargetValue,
                        }
                    )
                );
                await adsService.PostNewAdSubLog(adTarget.AdId, message, null, null);
            }

            // update attributes of the ad target
            adTarget.TargetType = request.TargetType;
            adTarget.TargetValue = request.TargetValue;
            adTarget.Weight = request.Weight;

            await context.SaveChangesAsync();

            await tx.CommitAsync();
        }

        /// <summary>
        /// Set an ad target as the primary ad target of the ad
        /// </summary>
        /// <param name="adTarget">ad target</param>
        /// <returns>the ad target id</returns>
        public async Task<int> SetAdTargetAsPrimary(Models.TravelTipsModels.AdTarget adTarget)
        {
            var adTargets = context.AdTargets.Where(at => at.AdId == adTarget.AdId).ToList();
            var primaryAdTarget = adTargets.FirstOrDefault(at => at.IsPrimary == true);

            if (primaryAdTarget != null)
            {
                primaryAdTarget.IsPrimary = false;
            }

            adTarget.IsPrimary = true;

            await context.SaveChangesAsync();
            return adTarget.Id;
        }

        /// <summary>
        /// delete an ad target
        /// </summary>
        /// <param name="adTarget">ad target</param>
        /// <returns></returns>
        public async Task DeleteAdTarget(Models.TravelTipsModels.AdTarget adTarget)
        {
            var oldWeight = adTarget.Weight;

            var tx = context.Database.BeginTransaction();

            context.AdTargets.Remove(adTarget);
            await context.SaveChangesAsync();

            // create a sub log for the ad target weight decrease
            var message = string.Format(Messages.AdTargetDeleted, GetAdTargetDescription(adTarget));
            await adsService.PostNewAdSubLog(adTarget.AdId, message, null, null);

            await tx.CommitAsync();
        }

        /// <summary>
        /// Update ad targets of an ad to the next cycle
        /// </summary>
        /// <param name="adId">ad id</param>
        /// <returns></returns>
        public async Task UpdateAdTargetCycleByAdId(int adId)
        {
            var tx = context.Database.BeginTransaction();

            // create a sub log for the new ad target cycle
            await adsService.PostNewAdSubLog(adId, Messages.AdNewCycle, null, null);

            await tx.CommitAsync();
        }

        private string GetAdTargetDescription(Models.TravelTipsModels.AdTarget adTarget)
        {
            return $"{adTarget.TargetType} - {adTarget.TargetValue}";
        }
    }
}
