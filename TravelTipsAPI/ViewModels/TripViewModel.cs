using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels
{
    public class TripViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public static explicit operator TripViewModel?(Trip? trip)
        {
            if (trip == null) return null;

            var tripViewModel = new TripViewModel
            {
                Id = trip.Id,
                Name = trip.Name,
                Description = trip.Description,
                CreatedBy = trip.CreatedBy,
                CreatedAt = trip.CreatedAt,
                LastUpdatedAt = trip.LastUpdatedAt,
            };

            return tripViewModel;
        }
    }
}
