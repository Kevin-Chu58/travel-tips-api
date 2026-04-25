using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class TargetRuleViewModel
    {
        public int Id { get; set; }
        public required string TargetType { get; set; }
        public string? TargetValue { get; set; }
        public int MinWeight { get; set; }

        public static explicit operator TargetRuleViewModel(TargetRule targetRule)
        {
            return new TargetRuleViewModel
            {
                Id = targetRule.Id,
                TargetType = targetRule.TargetType,
                TargetValue = targetRule.TargetValue,
                MinWeight = targetRule.MinWeight,
            };
        }
    }
}
