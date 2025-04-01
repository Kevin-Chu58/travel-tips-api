namespace TravelTipsAPI.ViewModels
{
    public class TripDetailViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public required IEnumerable<SmallTripViewModel> SmallTrips { get; set; }
    }
}
