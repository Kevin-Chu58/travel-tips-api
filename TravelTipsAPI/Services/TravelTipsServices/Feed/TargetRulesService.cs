using TravelTipsAPI.Constants;
using TravelTipsAPI.Constants.Enums;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Feed
{
    public class TargetRulesService(TravelTipsContext context) : ITargetRulesService
    {
        /// <summary>
        /// Find a target rule by id
        /// </summary>
        /// <param name="targetRuleId">target rule id</param>
        /// <returns>the target rule with the id</returns>
        public TargetRule? FindTargetRuleById(int targetRuleId)
        {
            return context.TargetRules.FirstOrDefault(tr => tr.Id == targetRuleId);
        }

        /// <summary>
        /// Get a list of target rules by target type
        /// </summary>
        /// <param name="targetType">target type</param>
        /// <returns>a list of target rules of the target type</returns>
        public IEnumerable<TargetRuleViewModel> GetTargetRulesByType(
            AdTargetEnum.AdTarget targetType
        )
        {
            var targetTypeStr = AdTargetEnum.GetAdTargetStr(targetType);

            return context
                .TargetRules.Where(tr => tr.TargetType == targetTypeStr)
                .Select(tr => (TargetRuleViewModel)tr)
                .ToList();
        }

        /// <summary>
        /// Get a target rule by target type and target value. If not found, return null
        /// </summary>
        /// <param name="targetType">target type</param>
        /// <param name="targetValue">target value</param>
        /// <returns>a target rule</returns>
        public TargetRuleViewModel? GetTargetRule(string targetTypeStr, string? targetValue)
        {
            return context
                .TargetRules.Where(tr =>
                    tr.TargetType == targetTypeStr && tr.TargetValue == targetValue
                )
                .Select(tr => (TargetRuleViewModel)tr)
                .FirstOrDefault();
        }

        /// <summary>
        /// Create a new target rule
        /// </summary>
        /// <param name="targetType">target type</param>
        /// <param name="targetValue">target value</param>
        /// <param name="MinWeight">min weight</param>
        /// <returns>the newly created target rule</returns>
        public async Task<TargetRuleViewModel> PostNewTargetRule(
            AdTargetEnum.AdTarget targetType,
            string? targetValue,
            int MinWeight
        )
        {
            var targetTypeStr = AdTargetEnum.GetAdTargetStr(targetType);

            if (targetTypeStr == null)
            {
                throw new Exception(Messages.AdTargetTypeInvalid);
            }

            var targetRule = new TargetRule
            {
                TargetType = targetTypeStr,
                TargetValue = targetValue,
                MinWeight = MinWeight,
            };

            context.TargetRules.Add(targetRule);
            await context.SaveChangesAsync();

            return (TargetRuleViewModel)targetRule;
        }

        /// <summary>
        /// Update an existing target rule
        /// </summary>
        /// <param name="targetRule">target rule</param>
        /// <param name="targetType">target type</param>
        /// <param name="targetValue">target value</param>
        /// <param name="minWeight">min weight</param>
        /// <returns>the updated target rule</returns>
        public async Task<TargetRuleViewModel> UpdateTargetRule(
            TargetRule targetRule,
            AdTargetEnum.AdTarget? targetType = null,
            string? targetValue = null,
            int? minWeight = null
        )
        {
            var targetTypeStr =
                targetType != null ? AdTargetEnum.GetAdTargetStr(targetType.Value) : null;

            targetRule.TargetType = targetTypeStr ?? targetRule.TargetType;
            targetRule.TargetValue = targetValue ?? targetRule.TargetValue;
            targetRule.MinWeight = minWeight ?? targetRule.MinWeight;

            context.TargetRules.Update(targetRule);
            await context.SaveChangesAsync();

            return (TargetRuleViewModel)targetRule;
        }

        /// <summary>
        /// Delete a target rule
        /// </summary>
        /// <param name="targetRule">target rule to be deleted</param>
        /// <returns>the deleted target rule id</returns>
        public async Task<int> DeleteTargetRule(TargetRule targetRule)
        {
            context.TargetRules.Remove(targetRule);
            await context.SaveChangesAsync();

            return targetRule.Id;
        }
    }
}
