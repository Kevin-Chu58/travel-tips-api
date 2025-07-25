using TravelTipsAPI.Models.TravelTipsModels;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class DayPostViewModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public int TripId { get; set; }

        public Day ToDay(int createdBy)
        {
            var day = new Day
            {
                Id = new int(),
                Title = Title?.Trim(),
                Description = Description?.Trim(),
                Start = Start,
                End = End,
                IsOverNight = Start > End,
                TripId = TripId,
                CreatedBy = createdBy,
            };

            return day;
        }
    }
}
