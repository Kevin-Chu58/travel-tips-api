namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripAttractionOrderGeoViewModel
    {
        public int Id { get; set; }
        public int DayId { get; set; }
        public required string Title { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}
