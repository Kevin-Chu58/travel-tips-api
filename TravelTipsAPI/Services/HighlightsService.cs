using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Highlights
    /// </summary>
    /// <param name="context">context</param>
    public class HighlightsService(TravelTipsContext context) : IHighlightsService
    {
        /// <summary>
        /// Find a highlight by id
        /// </summary>
        /// <param name="id">highlight id</param>
        /// <returns>the highlight with the id</returns>
        public Highlight FindHighlightById(int id)
        {
            var highlight = context.Highlights.Find(id);

            if (highlight == null)
                throw new FileNotFoundException(Messages.HighlightNotFound);

            return highlight;
        }

        /// <summary>
        /// Get a list of highlights by attraction id
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <param name="userId">user id</param>
        /// <returns>a list of highlights</returns>
        public IEnumerable<Highlight> GetHighlightsByParams(int id, int? userId)
        {
            var highlights = context.Highlights.Where(h => h.AttractionId == id).ToList();

            if (userId != null)
                highlights = [.. highlights.Where(h => h.CreatedBy == userId)];

            return highlights;
        }
    }
}
