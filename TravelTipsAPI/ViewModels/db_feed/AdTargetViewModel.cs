using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdTargetViewModel
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public required string TargetType { get; set; }
        public required string TargetValue { get; set; }
        public int Weight { get; set; }
        public bool IsPrimary { get; set; }

        public static explicit operator AdTargetViewModel(AdTarget adTarget)
        {
            return new AdTargetViewModel
            {
                Id = adTarget.Id,
                AdId = adTarget.AdId,
                TargetType = adTarget.TargetType,
                TargetValue = adTarget.TargetValue,
                Weight = adTarget.Weight,
                IsPrimary = adTarget.IsPrimary,
            };
        }
    }
}
