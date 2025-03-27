using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels
{
    public class TripPostViewModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public Trip ToTrip()
        {
            var trip = new Trip
            {
                Id = new int(),
                Name = this.Name,
                Description = this.Description,
                CreatedBy = this.CreatedBy,
                CreatedAt = this.CreatedAt,
                LastUpdatedAt = this.LastUpdatedAt,
            };
            return trip;
        }
    }
}
