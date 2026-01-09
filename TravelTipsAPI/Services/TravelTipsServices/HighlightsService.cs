using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    /// <summary>
    /// The service of Highlights
    /// </summary>
    /// <param name="context">context</param>
    public class HighlightsService(TravelTipsContext context, IUsersService usersService)
        : IHighlightsService
    {
        /// <summary>
        /// Find a highlight by id
        /// </summary>
        /// <param name="id">highlight id</param>
        /// <returns>the highlight with the id</returns>
        public Highlight? FindHighlightById(int id)
        {
            var highlight = context.Highlights.Find(id);

            return highlight;
        }

        /// <summary>
        /// Get a list of highlights by attraction id
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <param name="userId">user id</param>
        /// <returns>a list of highlights</returns>
        public IEnumerable<Highlight> GetHighlightsByParams(int id, int? userId = null)
        {
            var query = context.Highlights.AsQueryable();

            query = query.Where(h => h.AttractionId == id);
            if (userId != null)
            {
                query = query.Where(h => h.CreatedBy == userId);
            }

            return query.ToList();
        }

        /// <summary>
        /// Get Highlight view model with user information if exists
        /// </summary>
        /// <param name="highlight">highlight</param>
        /// <returns>highlight view model</returns>
        public HighlightViewModel GetHighlightViewModel(Highlight highlight)
        {
            var highlightViewModel = (HighlightViewModel)highlight;

            var userViewModel = (UserViewModel)usersService.GetUserById(highlight.CreatedBy);

            highlightViewModel.CreatedBy = userViewModel;

            return highlightViewModel;
        }

        /// <summary>
        /// Get my highlight ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of my highlight ids</returns>
        public IEnumerable<int> GetMyHighlights(int id)
        {
            var myHighlightIds = context
                .Highlights.Where(h => h.CreatedBy == id)
                .Select(h => h.Id)
                .ToList();

            return myHighlightIds;
        }

        /// <summary>
        /// Create a new highlight
        /// </summary>
        /// <param name="newHighlight">new highlight detail</param>
        /// <param name="userId">user id</param>
        /// <returns>a new highlight</returns>
        public async Task<HighlightViewModel> PostNewHighlightAsync(
            HighlightPostViewModel newHighlight,
            int userId
        )
        {
            var highlight = newHighlight.ToHighlight(userId);

            await context.Highlights.AddAsync(highlight);
            await context.SaveChangesAsync();

            return GetHighlightViewModel(highlight);
        }

        /// <summary>
        /// update an existing highlight
        /// </summary>
        /// <param name="highlight">highlight to be updated</param>
        /// <param name="description">new description</param>
        /// <returns>the updated highlight</returns>
        public async Task<HighlightViewModel> UpdateHighlightAsync(
            Highlight highlight,
            string description
        )
        {
            if (description.Length == 0)
                throw new Exception(Messages.HighlightDescriptionEmpty);

            highlight.Description = description;
            await context.SaveChangesAsync();

            return GetHighlightViewModel(highlight);
        }

        /// <summary>
        /// Delete an existing highlight
        /// </summary>
        /// <param name="highlight">highlight</param>
        /// <returns>the deleted highlight</returns>
        public async Task<HighlightViewModel> DeleteHighlightAsync(Highlight highlight)
        {
            // replace all reference to this highlight with null


            var taos = context.TripAttractionOrders.Where(tao => tao.HighlightId == highlight.Id);
            foreach (var tao in taos)
            {
                tao.HighlightId = null;
            }

            context.Highlights.Remove(highlight);
            await context.SaveChangesAsync();

            return GetHighlightViewModel(highlight);
        }
    }
}
