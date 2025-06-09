using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public bool IsPublic { get; set; }
        public int? NumDays { get; set; }

        public static explicit operator TripViewModel(Trip trip)
        {
            return new TripViewModel
            {
                Id = trip.Id,
                Name = trip.Name,
                Description = trip.Description,
                CreatedBy = trip.CreatedBy,
                CreatedAt = trip.CreatedAt,
                LastUpdatedAt = trip.LastUpdatedAt,
                IsPublic = trip.IsPublic,
            };
        }
    }
}
