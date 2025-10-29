using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class DayViewModel
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public int TripId { get; set; }

        public static explicit operator DayViewModel(Day day)
        {
            var dayViewModel = new DayViewModel
            {
                Id = day.Id,
                Description = day.Description,
                TripId = day.TripId,
            };

            return dayViewModel;
        }
    }
}
