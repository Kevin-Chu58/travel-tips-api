using TravelTipsAPI.Models;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class DayPostViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public TimeOnly Start {  get; set; }
        public TimeOnly End { get; set; }
        public int TripId { get; set; }

        public Day ToDay(int createdBy)
        {
            var day = new Day
            {
                Id = new int(),
                Name = Name,
                Description = Description,
                Start = Start,
                End = End,
                IsOverNight = Start > End,
                TripId = TripId,
                CreatedBy = createdBy
            };

            return day;
        }
    }
}
