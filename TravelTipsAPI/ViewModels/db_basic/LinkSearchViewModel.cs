namespace TravelTipsAPI.ViewModels.db_basic
{
    public class LinkSearchViewModel
    {
        public long Timestamp { get; set; }
        public required IEnumerable<LinkViewModel> Links { get; set; }
    }
}
