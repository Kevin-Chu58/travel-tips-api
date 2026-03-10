using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_image;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BannerViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Overview { get; set; }
        public ImageViewModel? Picture { get; set; }
        public required string Link { get; set; }
        public DateOnly From { get; set; }
        public DateOnly? To { get; set; }
        public string? Label { get; set; }
        public string? SubLabel { get; set; }
        public BannerStylingViewModel? Styling { get; set; }

        public static explicit operator BannerViewModel(Banner banner)
        {
            var bannerSimple = new BannerViewModel
            {
                Id = banner.Id,
                Title = banner.Title,
                Overview = banner.Overview,
                Link = banner.Link,
                From = banner.From,
                To = banner.To,
                Label = banner.Label,
                SubLabel = banner.SubLabel,
            };

            return bannerSimple;
        }
    }
}
