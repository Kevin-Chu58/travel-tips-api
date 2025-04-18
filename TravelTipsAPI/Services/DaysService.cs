using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Days
    /// </summary>
    /// <param name="context">context</param>
    public class DaysService(TravelTipsContext context) : IDaysService
    {
        /// <summary>
        /// Get day by its id
        /// </summary>
        /// <param name="id">day id</param>
        /// <returns>the day with the id, null if not found</returns>
        public DayViewModel? GetDayById(int id)
        {
            var day = context.Days.Find(id);

            return (DayViewModel)day;
        }

        /// <summary>
        /// Get days by trip id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns>days with the trip id</returns>
        public IEnumerable<DayViewModel> GetDaysByTripId(int tripId)
        {
            var dayViewModels = context.Days
                .Where(day => day.TripId == tripId)
                .Select(day => (DayViewModel)day)
                .ToList();

            return dayViewModels;
        }

        /// <summary>
        /// Get your days' ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of the ids of days you own</returns>
        public IEnumerable<int> GetYourDayIds(int id)
        {
            var yourDayIds = context.Days
                .Where (day => day.CreatedBy == id)
                .Select(day => day.Id)
                .ToList();

            return yourDayIds;
        }

        /// <summary>
        /// Create a new day
        /// </summary>
        /// <param name="newDay">new day detail</param>
        /// <returns>the new day</returns>
        public async Task<DayViewModel> PostNewDayAsync(int createdBy, DayPostViewModel newDay)
        {
            var day = newDay.ToDay(createdBy);

            if (day.Start == day.End)
                throw DaysStartEndOrderException();

            await context.Days.AddAsync(day);
            await context.SaveChangesAsync();

            return (DayViewModel)day;
        }

        /// <summary>
        /// Update the day detail by its id
        /// </summary>
        /// <param name="id">day id</param>
        /// <param name="day">day detail to update</param>
        /// <returns>the updated day</returns>
        public async Task<DayViewModel> PatchDayAsync(int id, DayPatchViewModel dayPatchViewModel)
        {
            var day = context.Days.Find(id);

            day.Name = dayPatchViewModel.Name ?? day.Name;
            day.Description = dayPatchViewModel.Description ?? day.Description;
            day.Start = dayPatchViewModel.Start ?? day.Start;
            day.End = dayPatchViewModel.End ?? day.End;
            day.IsOverNight = day.Start > day.End;

            if (day.Start == day.End) 
                throw DaysStartEndOrderException();

            await context.SaveChangesAsync();

            return (DayViewModel)day;
        }

        /// <summary>
        /// Get the exception of day id not found
        /// </summary>
        /// <param name="id">day id</param>
        /// <returns>an exception</returns>
        private static Exception DaysIdNotFoundException(int id)
        {
            return new Exception($"Day not found with id {id}");
        }

        /// <summary>
        /// Get the exception of day id not found
        /// </summary>
        /// <param name="id">day id</param>
        /// <returns>an exception</returns>
        private static Exception DaysStartEndOrderException()
        {
            return new Exception("Start time cannot equal to End time");
        }
    }
}
