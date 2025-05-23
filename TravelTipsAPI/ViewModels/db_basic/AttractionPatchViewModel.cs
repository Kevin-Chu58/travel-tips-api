namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionPatchViewModel
    {
        // attractions
        public long? OsmId { get; set; }
        public decimal? Lng { get; set; }
        public decimal? Lat { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }

        // highlights
        public string? Description { get; set; }
        public int? LinkId { get; set; }
    }
}
