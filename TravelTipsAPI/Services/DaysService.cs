using TravelTipsAPI.Constants;
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
        /// Find day by its id
        /// </summary>
        /// <param name="id">day id</param>
        /// <returns>the day with the id</returns>
        public Day FindDayById(int id, bool? isPublic = null)
        {
            var day = context.Days.Find(id);

            if (day is null || (isPublic != null && day.Trip.IsPublic != isPublic))
                throw new Exception(Messages.DayNotFound);

            return day;
        }

        /// <summary>
        /// Get days by public trip id
        /// </summary>
        /// <param name="tripId">trip id</param>
        /// <returns>days with the trip id</returns>
        public IEnumerable<DayViewModel> GetDaysByTripId(int tripId)
        {
            var trip = context.Trips.Find(tripId);
            if (trip?.IsPublic == false)
                throw new Exception(Messages.TripNotFound);

            var dayViewModels = context
                .Days.Where(day => day.TripId == tripId)
                .Select(day => (DayViewModel)day)
                .ToList();

            return dayViewModels;
        }

        /// <summary>
        /// Get my days' ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of the ids of days I own</returns>
        public IEnumerable<int> GetMyDayIds(int id)
        {
            var myDayIds = context
                .Days.Where(day => day.CreatedBy == id)
                .Select(day => day.Id)
                .ToList();

            return myDayIds;
        }

        /// <summary>
        /// Create a new day
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="newDay">new day detail</param>
        /// <returns>the new day</returns>
        public async Task<DayViewModel> PostNewDayAsync(int createdBy, DayPostViewModel newDay)
        {
            var day = newDay.ToDay(createdBy);

            if (day.Start == day.End)
                throw new Exception(Messages.Day24HourRestricted);

            if (!DoesDayEndBeforeStart(day))
                throw new Exception(Messages.DayStartsBeforeEndRestricted);

            await context.Days.AddAsync(day);
            await context.SaveChangesAsync();

            return (DayViewModel)day;
        }

        /// <summary>
        /// Update the day detail by its id
        /// </summary>
        /// <param name="day">day</param>
        /// <param name="dayPatch">day detail to update</param>
        /// <returns>the updated day</returns>
        public async Task<DayViewModel> PatchDayAsync(Day day, DayPatchViewModel dayPatch)
        {
            day.Name = dayPatch.Name?.Trim() ?? day.Name;
            day.Description = dayPatch.Description?.Trim() ?? day.Description;
            day.Start = dayPatch.Start ?? day.Start;
            day.End = dayPatch.End ?? day.End;
            day.IsOverNight = day.Start > day.End;

            if (day.Start == day.End)
                throw new Exception(Messages.Day24HourRestricted);

            if (!DoesDayEndBeforeStart(day, day.Id))
                throw new Exception(Messages.DayStartsBeforeEndRestricted);

            await context.SaveChangesAsync();

            return (DayViewModel)day;
        }

        /// <summary>
        /// Check if new day's detail is valid
        /// </summary>
        /// <param name="newDay">new day</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePost(DayPostViewModel newDay)
        {
            var invalidParams = new List<string>();

            if (newDay.Name?.Length > 50)
                invalidParams.Add("name");
            if (newDay.Description?.Length > 500)
                invalidParams.Add("description");

            return invalidParams;
        }

        /// <summary>
        /// Check if day's detail is valid
        /// </summary>
        /// <param name="day">existing day</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePatch(DayPatchViewModel day)
        {
            var invalidParams = new List<string>();

            if (day.Name?.Length > 50)
                invalidParams.Add("name");
            if (day.Description?.Length > 500)
                invalidParams.Add("description");

            return invalidParams;
        }

        /// <summary>
        /// Check if yesterday ends before the new day's dawning
        /// </summary>
        /// <param name="today">today</param>
        /// <param name="dayId"> next day id</param>
        /// <returns>true if ends before next start, false otherwise</returns>
        private bool DoesDayEndBeforeStart(Day today, int? dayId = null)
        {
            var days = context.Days.Where(day => day.TripId == today.TripId).ToList();
            // if no days exist in the trip, always return true
            if (days.Count == 0)
                return true;

            // POST new day - checks only yesterday
            // PATCH day - checks both yesterday and tomorrow

            List<Day> daysBefore,
                daysAfter;
            Day yesterday,
                tomorrow;

            if (dayId is null)
            {
                yesterday = days.OrderBy(day => day.Id).Last();
                return !yesterday.IsOverNight || yesterday.End < today.Start;
            }
            else
            {
                daysBefore = [.. days.Where(day => day.Id < dayId)];
                daysAfter = [.. days.Where(day => day.Id > dayId)];

                // the status of end before start restriction is applied
                bool isRestricted = true;

                if (daysBefore.Count > 0)
                {
                    yesterday = daysBefore.OrderBy(day => day.Id).Last();
                    isRestricted &= !yesterday.IsOverNight || yesterday.End < today.Start;
                }
                if (daysAfter.Count > 0)
                {
                    tomorrow = daysAfter.OrderBy(day => day.Id).First();
                    isRestricted &= !today.IsOverNight || today.End < tomorrow.Start;
                }

                return isRestricted;
            }
        }
    }
}
