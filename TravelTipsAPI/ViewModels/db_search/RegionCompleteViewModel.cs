using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_search
{
    public class RegionCompleteViewModel
    {
        public RegionViewModel? Continent { get; set; }
        public RegionViewModel? Country { get; set; }
        public RegionViewModel? State { get; set; }
        public RegionViewModel? Area { get; set; }
    }
}
