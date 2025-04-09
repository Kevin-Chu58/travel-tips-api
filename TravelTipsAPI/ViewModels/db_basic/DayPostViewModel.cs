using TravelTipsAPI.Models.Basic;

namespace TravelTipsAPI.ViewModels.db_basic
{
    public class DayPostViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public TimeOnly Start {  get; set; }
        public TimeOnly End { get; set; }

        public Day ToDay(int tripId, int createdBy)
        {
            var day = new Day
            {
                Id = new int(),
                Name = Name,
                Description = Description,
                Start = Start,
                End = End,
                IsOverNight = Start > End,
                TripId = tripId,
                CreatedBy = createdBy
            };

            return day;
        }
    }
}
