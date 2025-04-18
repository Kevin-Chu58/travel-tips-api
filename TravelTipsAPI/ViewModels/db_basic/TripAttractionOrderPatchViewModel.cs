namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripAttractionOrderPatchViewModel
    {
        public int? DayId { get; set; }
        public int? AttractionId { get; set; }
        public int? EstimateTime { get; set; }
        public bool? IsDrivePreferred { get; set; }
        public bool? IsBikePreferred { get; set; }
        public bool? IsOnFootPreferred { get; set; }
    }
}
