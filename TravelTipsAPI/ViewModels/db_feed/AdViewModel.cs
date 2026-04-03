using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class AdViewModel
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public int ImageId { get; set; }
        public required string Title { get; set; }
        public string? Text { get; set; }
        public string? ButtonLabel { get; set; }
        public string? Link { get; set; }
        public string? Picture { get; set; }
        public string? SubStatus { get; set; }
        public required string Status { get; set; }

        public static explicit operator AdViewModel(Ad ad)
        {
            return new AdViewModel
            {
                Id = ad.Id,
                BusinessId = ad.BusinessId,
                ImageId = ad.ImageId,
                Title = ad.Title,
                Text = ad.Text,
                ButtonLabel = ad.ButtonLabel,
                Link = ad.Link,
                SubStatus = ad.SubStatus,
                Status = ad.Status,
            };
        }
    }
}
