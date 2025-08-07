namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripAttractionOrderViewModel
    {
        public int Id { get; set; }
        public int DayId { get; set; }
        public required Attraction2ViewModel Attraction { get; set; }
        public HighlightViewModel? Highlight { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public int CreatedBy { get; set; }
        //public required IEnumerable<PreferRouteViewModel> PreferRoutes { get; set; }
    }
}
