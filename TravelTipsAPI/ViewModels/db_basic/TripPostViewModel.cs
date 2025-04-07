using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripPostViewModel
    {
        public required string Name { get; set; }
        public string? Description { get; set; }

        public Trip ToTrip(int CreatedBy)
        {
            var trip = new Trip
            {
                Id = new int(),
                Name = Name.Trim(),
                Description = Description?.Trim(),
                CreatedBy = CreatedBy,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now,
            };
            return trip;
        }
    }
}
