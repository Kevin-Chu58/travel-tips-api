namespace TravelTipsAPI.ViewModels
{
    public class PreferRoutePostViewModel
    {
        public int Type { get; set; }
        public required string Ref {  get; set; }
        public int CreatedBy {  get; set; }
        public int DepartOsmId { get; set; }
        public int ArrivalOsmId { get; set; }
        public int EstimateTime { get; set; }
        public int? LinkId { get; set; }
    }
}
