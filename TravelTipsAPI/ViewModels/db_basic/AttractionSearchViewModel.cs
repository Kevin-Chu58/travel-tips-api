namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionSearchViewModel
    {
        public int Timestamp { get; set; }
        public required IEnumerable<AttractionViewModel> Attractions { get; set; }
    }
}
