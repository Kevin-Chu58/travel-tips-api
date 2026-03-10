using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BannerStylingViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Styling { get; set; }

        public static explicit operator BannerStylingViewModel(BannerStyling bannerStyling)
        {
            return new BannerStylingViewModel
            {
                Id = bannerStyling.Id,
                Name = bannerStyling.Name,
                Styling = bannerStyling.Styling,
            };
        }
    }
}
