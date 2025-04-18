namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionSearchViewModel
    {
        public int TimeStamp { get; set; }
        public required IEnumerable<AttractionViewModel> Attractions { get; set; }
    }
}
