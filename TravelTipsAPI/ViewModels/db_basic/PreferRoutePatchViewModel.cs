namespace TravelTipsAPI.ViewModels.db_basic
{
    public class PreferRoutePatchViewModel
    {
        public int? Type { get; set; }
        public string? Ref {  get; set; }
        public int? DepartOsmId { get; set; }
        public int? ArrivalOsmId { get; set; }
        public int? EstimateTime { get; set; }
        public int? LinkId { get; set; }
    }
}
