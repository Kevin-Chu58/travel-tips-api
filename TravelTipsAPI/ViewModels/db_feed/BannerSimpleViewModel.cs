using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BannerSimpleViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public DateOnly From { get; set; }
        public DateOnly? To { get; set; }
        public string? Label { get; set; }
        public string? SubLabel { get; set; }

        public static explicit operator BannerSimpleViewModel(Banner banner)
        {
            var bannerSimple = new BannerSimpleViewModel
            {
                Id = banner.Id,
                Title = banner.Title,
                From = banner.From,
                To = banner.To,
                Label = banner.Label,
                SubLabel = banner.SubLabel,
            };

            return bannerSimple;
        }
    }
}
