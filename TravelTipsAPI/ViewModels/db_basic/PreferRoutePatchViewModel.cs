namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRoutePatchViewModel
    {
        public int? Type { get; set; }
        public AttractionViewModel? DepartAttraction { get; set; }
        public AttractionViewModel? ArrivalAttraction { get; set; }
        public int? EstimateTime { get; set; }
        public int? LinkId { get; set; }
    }
}
