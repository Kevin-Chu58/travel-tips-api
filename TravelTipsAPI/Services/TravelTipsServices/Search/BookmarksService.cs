using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Search
{
    public class BookmarksService(TravelTipsContext context) : IBookmarksService
    {
        /// <summary>
        /// Get a list of bookmarked trip ids by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>a list of bookmarked trip ids</returns>
        public IEnumerable<int> GetBookmarkTripIdsByUserId(int userId)
        {
            var bookmarkTripIds = context
                .Bookmarks.Where(bookmark => bookmark.UserId == userId)
                .Select(bookmark => bookmark.TripId)
                .ToList();

            return bookmarkTripIds;
        }

        /// <summary>
        /// Check if a user has already bookmarked a trip
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="tripId">trip id</param>
        /// <returns>whether a user has already bookmarked a trip</returns>
        public bool IsBookmarked(int userId, int tripId)
        {
            var isBookmarked = context.Bookmarks.Any(bookmark =>
                bookmark.UserId == userId && bookmark.TripId == tripId
            );
            return isBookmarked;
        }

        /// <summary>
        /// Add bookmark on a trip for a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="tripId">trip id</param>
        /// <returns></returns>
        public async Task AddBookmarkAsync(int userId, int tripId)
        {
            var isBookmarked = IsBookmarked(userId, tripId);
            if (isBookmarked)
                throw new Exception(Messages.BookmarkAlreadyExists);

            var bookmark = new Bookmark { UserId = userId, TripId = tripId };
            await context.Bookmarks.AddAsync(bookmark);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove bookmark on a trip for a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="tripId">trip id</param>
        /// <returns></returns>
        public async Task RemoveBookmarkAsync(int userId, int tripId)
        {
            var isBookmarked = IsBookmarked(userId, tripId);
            if (!isBookmarked)
                throw new Exception(Messages.BookmarkNotFound);

            var bookmark = context.Bookmarks.FirstOrDefault(b =>
                b.UserId == userId && b.TripId == tripId
            );
            context.Bookmarks.Remove(bookmark!);
            await context.SaveChangesAsync();
        }
    }
}
