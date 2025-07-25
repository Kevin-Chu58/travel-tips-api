using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class TripViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public UserViewModel? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public int? NumDays { get; set; }

        public static explicit operator TripViewModel(Trip trip)
        {
            return new TripViewModel
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                CreatedAt = trip.CreatedAt,
                IsPublic = trip.IsPublic,
            };
        }
    }
}
