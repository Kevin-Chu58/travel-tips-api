namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripAttractionOrderViewModel
    {
        public int Id { get; set; }
        public int DayId { get; set; }
        public int Order {  get; set; }
        public int AttractionId { get; set; }
        public int EstimateTime { get; set; }
        public int CreatedBy { get; set; }
        public bool IsDrivePreferred { get; set; }
        public bool IsBikePreferred { get; set; }
        public bool IsOnFootPreferred { get; set; }
        public required IEnumerable<PreferRouteViewModel> PreferRoutes { get; set; }
    }
}
