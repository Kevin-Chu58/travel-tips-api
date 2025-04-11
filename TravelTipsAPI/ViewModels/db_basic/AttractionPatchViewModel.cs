namespace TravelTipsAPI.ViewModels.db_basic
{
    public class AttractionPatchViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int? OsmId { get; set; }
        public int? LinkId { get; set; }
    }
}
