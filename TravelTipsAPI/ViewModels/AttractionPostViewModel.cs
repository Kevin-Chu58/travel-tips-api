namespace TravelTipsAPI.ViewModels
{
    public class AttractionPostViewModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Address { get; set; }
        public int CreatedBy { get; set; }
        public int OsmId { get; set; }
        public int? LinkId { get; set; }
    }
}
