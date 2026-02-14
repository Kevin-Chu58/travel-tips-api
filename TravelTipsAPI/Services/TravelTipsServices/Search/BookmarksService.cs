using Microsoft.EntityFrameworkCore;
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
        /// Add bookmark on a trip for a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="tripId">trip id</param>
        /// <returns></returns>
        public async Task AddBookmarkAsync(int userId, int tripId)
        {
            var bookmark = await context.Bookmarks.FirstOrDefaultAsync(b =>
                b.UserId == userId && b.TripId == tripId
            );
            if (bookmark != null)
                throw new Exception(Messages.BookmarkAlreadyExists);

            bookmark = new Bookmark { UserId = userId, TripId = tripId };
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
            var bookmark = await context.Bookmarks.FirstOrDefaultAsync(b =>
                b.UserId == userId && b.TripId == tripId
            );

            if (bookmark is null)
                throw new Exception(Messages.BookmarkNotFound);

            context.Bookmarks.Remove(bookmark!);
            await context.SaveChangesAsync();
        }
    }
}
