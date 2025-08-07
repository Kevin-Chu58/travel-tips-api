namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripAttractionOrderPatchViewModel
    {
        public int? DayId { get; set; }
        public int? AttractionId { get; set; }
        public int? HighlightId { get; set; }
        public TimeOnly? Start { get; set; }
        public TimeOnly? End { get; set; }
    }
}
