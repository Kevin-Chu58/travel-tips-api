namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRoutePatchViewModel
    {
        public int? Type { get; set; }
        public long? DepartOsmId { get; set; }
        public long? ArrivalOsmId { get; set; }
        public int? EstimateTime { get; set; }
        public int? LinkId { get; set; }
    }
}
