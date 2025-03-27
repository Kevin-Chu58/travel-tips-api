namespace TravelTipsAPI.ViewModels
{
    public class TripAttractionOrderPostViewModel
    {
        public int DayId { get; set; }
        public int Order {  get; set; }
        public int AttractionId { get; set; }
        public int EstimateTime { get; set; }
        public bool IsDrivePreferred { get; set; }
        public bool IsBikePreferred { get; set; }
        public required int[] Routes { get; set; }
    }
}
