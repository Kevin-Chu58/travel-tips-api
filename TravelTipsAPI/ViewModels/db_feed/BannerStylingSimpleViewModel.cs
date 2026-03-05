using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_feed
{
    public class BannerStylingSimpleViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public static explicit operator BannerStylingSimpleViewModel(BannerStyling bannerStyling)
        {
            return new BannerStylingSimpleViewModel
            {
                Id = bannerStyling.Id,
                Name = bannerStyling.Name,
            };
        }
    }
}
