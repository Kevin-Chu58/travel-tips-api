using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class DayViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public bool IsOverNight { get; set; }
        public int TripId { get; set; }
        public int CreatedBy { get; set; }

        public static explicit operator DayViewModel?(Day? day)
        {
            if (day == null) return null;

            var dayViewModel = new DayViewModel
            {
                Id = day.Id,
                Name = day.Name,
                Description = day.Description,
                Start = day.Start,
                End = day.End,
                IsOverNight = day.IsOverNight,
                TripId = day.TripId,
                CreatedBy = day.CreatedBy
            };

            return dayViewModel;
        }
    }
}
