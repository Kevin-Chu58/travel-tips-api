using TravelTipsAPI.ViewModels.db_basic;

namespace TravelTipsAPI.Models.Basic
{
    public class LinkSearchViewModel
    {
        public int TimeStamp { get; set; }
        public required IEnumerable<LinkViewModel> Links { get; set; }
    }
}
