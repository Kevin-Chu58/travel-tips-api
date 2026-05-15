using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Feed
{
    public class TripFeedsService(TravelTipsContext context) : ITripFeedsService
    {
        /// <summary>
        /// Retrieves a list of trip IDs that belong to a specified category.
        /// </summary>
        /// <param name="category">The category used to filter the trips.</param>
        /// <returns>A list of trip ids that matches the category</returns>
        public IEnumerable<int> GetTripIdsByCategory(string category)
        {
            return context
                .TripFeeds.Where(tf => tf.Category == category)
                .Select(tf => tf.TripId)
                .ToList();
        }
    }
}
