using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_search
{
    public class RegionCompleteViewModel
    {
        public Region? Continent { get; set; }
        public Region? Country { get; set; }
        public Region? State { get; set; }
        public Region? Area { get; set; }
    }
}
