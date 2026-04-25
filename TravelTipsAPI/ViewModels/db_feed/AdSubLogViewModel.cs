using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdSubLogViewModel
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public DateTimeOffset Time { get; set; }
        public required string Note { get; set; }
        public int? OldValue { get; set; }
        public int? NewValue { get; set; }

        public static explicit operator AdSubLogViewModel(AdSubLog adSubLog)
        {
            return new AdSubLogViewModel
            {
                Id = adSubLog.Id,
                AdId = adSubLog.AdId,
                Time = adSubLog.Time,
                Note = adSubLog.Note,
                OldValue = adSubLog.OldValue,
                NewValue = adSubLog.NewValue,
            };
        }
    }
}
