using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Constants.OrderBy.HighlightOrderBy;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

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
        /// <param name="attractionId">attraction id</param>
        /// <param name="createdBy">user id</param>
        /// <param name="cursor">highlight cursor</param>
        /// <param name="highlightOrderByEnum">order by enum</param>
        /// <param name="limit">limit</param>
        /// <returns>a list of highlights</returns>
        public IEnumerable<HighlightViewModel> GetHighlightsByParams(
            int? attractionId = null,
            int? createdBy = null,
            HighlightCursor? cursor = null,
            HighlightOrderByEnum? highlightOrderByEnum = null,
            int? limit = null
        )
        {
            var query = context.Highlights.AsQueryable();

            if (attractionId != null)
            {
                query = query.Where(h => h.AttractionId == attractionId);
            }
            if (createdBy != null)
            {
                query = query.Where(h => h.CreatedBy == createdBy);
            }

            // still order the query even if cursor is null
            if (highlightOrderByEnum != null)
            {
                query = ApplyCursor(query, cursor, highlightOrderByEnum);
            }

            if (limit != null)
            {
                query = query.Take(limit.Value);
            }
            var highlights = query.ToList();

            return highlights.Select(h => GetHighlightViewModel(h));
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
        /// Apply cursor to the query
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="cursor">highlight cursor</param>
        /// <param name="highlightOrderByEnum">order by</param>
        /// <returns>the query with applied cursor</returns>
        public static IQueryable<Highlight> ApplyCursor(
            IQueryable<Highlight> query,
            HighlightCursor? cursor,
            HighlightOrderByEnum? highlightOrderByEnum
        )
        {
            // Apply cursor and sort query based on order by enum
            switch (highlightOrderByEnum)
            {
                case HighlightOrderByEnum.Newest:
                    query = query.OrderByDescending(h => h.Id);
                    if (cursor != null)
                        query = query.Where(h => h.Id < cursor.Id);
                    break;

                case HighlightOrderByEnum.Oldest:
                    query = query.OrderBy(h => h.Id);
                    if (cursor != null)
                        query = query.Where(h => h.Id > cursor.Id);
                    break;

                case HighlightOrderByEnum.MostUsed:
                    query = query.OrderByDescending(h => h.UsageCount).ThenByDescending(h => h.Id);
                    if (cursor != null)
                        query = query.Where(h =>
                            h.UsageCount < cursor.UsageCount
                            || (h.UsageCount == cursor.UsageCount && h.Id < cursor.Id)
                        );
                    break;

                case HighlightOrderByEnum.LeastUsed:
                    query = query.OrderBy(h => h.UsageCount).ThenBy(h => h.Id);
                    if (cursor != null)
                        query = query.Where(h =>
                            h.UsageCount > cursor.UsageCount
                            || (h.UsageCount == cursor.UsageCount && h.Id > cursor.Id)
                        );
                    break;
            }

            return query;
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
            highlight.Description = description;
            await context.SaveChangesAsync();

            return GetHighlightViewModel(highlight);
        }

        /// <summary>
        /// Update highlight usage count when a TAO's highlight is changed
        /// </summary>
        /// <param name="oldId">old highlight id</param>
        /// <param name="newId">new highlight id</param>
        /// <returns></returns>
        public async Task UpdateHighlightUsageCountAsync(int? oldId, int? newId)
        {
            if (oldId == newId)
                return;

            if (oldId != null)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE db_basic.Highlights SET UsageCount = UsageCount - 1 WHERE Id = @id",
                    new SqlParameter("@id", oldId)
                );
            }
            if (newId != null)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "UPDATE db_basic.Highlights SET UsageCount = UsageCount + 1 WHERE Id = @id",
                    new SqlParameter("@id", newId)
                );
            }
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
