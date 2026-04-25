using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of Days
    /// </summary>
    /// <param name="context">context</param>
    public class DaysService(TravelTipsContext context) : IDaysService
    {
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
        /// Find day by its id
        /// </summary>
        /// <param name="id">day id</param>
        /// <returns>the day with the id</returns>
        public Day FindDayById(int id, bool? isPublic = null)
        {
            var day = context.Days.Find(id);

            if (day is null || isPublic != null && day.Trip.IsPublic != isPublic)
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
            var dayViewModels = context
                .Days.Where(day => day.TripId == tripId)
                .OrderBy(day => day.Id)
                .Select(day => (DayViewModel)day)
                .ToList();

            return dayViewModels;
        }

        /// <summary>
        /// Create a new day
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="tripId">trip id</param>
        /// <returns>the new day</returns>
        public async Task<DayViewModel> PostNewDayAsync(int createdBy, int tripId)
        {
            var day = new Day { CreatedBy = createdBy, TripId = tripId };

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
            day.Description = dayPatch.Description?.Trim() ?? day.Description;

            await context.SaveChangesAsync();

            return (DayViewModel)day;
        }

        /// <summary>
        /// Delete a day from database
        /// </summary>
        /// <param name="day">day</param>
        /// <returns>the day deleted</returns>
        public async Task<DayViewModel> DeleteDay(Day day)
        {
            var dayViewModel = (DayViewModel)day;

            context.Days.Remove(day);
            await context.SaveChangesAsync();

            return dayViewModel;
        }

        // subscriptions

        /// <summary>
        /// Check if a user can modify a day based on max trip count in their subscription
        /// </summary>
        /// <param name="dayId">day id</param>
        /// <param name="userId">user id</param>
        /// <returns>whether user can modify the day</returns>
        public bool CanUserEditDay(int dayId, int userId)
        {
            var maxTripCount = context
                .UserSubExtends.Where(use => use.UserId == userId)
                .Select(use => use.MaxTripCount)
                .FirstOrDefault();

            // only check the most recent maxTripCount trips due to subsctipion status
            return context
                .Trips.Where(t => t.CreatedBy == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Take(maxTripCount)
                .Any(t => t.Days.Any(d => d.Id == dayId));
        }
    }
}
