namespace TravelTipsAPI.ViewModels.db_basic
{
    public class LinkSearchViewModel
    {
        public int TimeStamp { get; set; }
        public required IEnumerable<LinkViewModel> Links { get; set; }
    }
}
