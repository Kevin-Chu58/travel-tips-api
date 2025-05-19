using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Attractions
    /// </summary>
    /// <param name="context">context</param>
    public class AttractionsService(TravelTipsContext context) : IAttractionsService
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
                throw new Exception(Messages.AttractionNotFound);

            return highlight;
        }

        /// <summary>
        /// Get a list of highlights by search params
        /// </summary>
        /// <param name="name">name to search</param>
        /// <param name="osmId">osm id</param>
        /// <param name="ownerId">user id</param>
        /// <returns>a list of highlights that satisfy the search params</returns>
        public IEnumerable<AttractionViewModel> GetHighlightsByParams(
            string? name,
            long? osmId,
            int? ownerId
        )
        {
            name = name?.Trim().ToLower();

            var attractionViewModels = new List<AttractionViewModel>();

            IEnumerable<Attraction> attractions = context.Attractions.ToList();

            if (name != null)
            {
                if (name.Length < SearchConstraints.ATTRACTION_SEARCH_MIN_LENGTH)
                    attractions = [];
                else
                    attractions = attractions.Where(a => a.Name.ToLower().Contains(name));
            }
            if (osmId != null)
                attractions = attractions.Where(a => a.OsmId == osmId);

            foreach (var attraction in attractions)
            {
                // exclude default highlights
                var highlights = context.Highlights.Where(h =>
                    h.AttractionId == attraction.Id && h.CreatedBy != null
                );

                // filter by the createdBy param
                if (ownerId != null)
                    highlights = highlights.Where(h => h.CreatedBy == ownerId);

                var viewModels = highlights
                    .Select(h => ToAttractionViewModel(h, attraction))
                    .ToList();

                attractionViewModels = [.. attractionViewModels, .. viewModels];
            }

            return attractionViewModels;
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
        /// <param name="createdBy">user id</param>
        /// <param name="attractionPost">new attraction</param>
        /// <returns>the new attraction created</returns>
        public async Task<AttractionViewModel> PostNewHighlightAsync(
            int? createdBy,
            AttractionPostViewModel attractionPost
        )
        {
            // check if osmId exists
            var attraction = context.Attractions.FirstOrDefault(
                (a) => a.OsmId == attractionPost.OsmId
            );
            var isDefault = attractionPost.Description is null && attractionPost.LinkId is null;

            // if exist, check whether is default
            if (attraction != null)
            {
                // if attraction exists, then default highlight also exists because they are created together
                var defaultHighlight = GetDefaultHighlight(attraction.Id);

                // if is default, return default highlight
                if (isDefault)
                {
                    return ToAttractionViewModel(defaultHighlight!, attraction);
                }
                // if is not default, create a custom highlight and return it
                else
                {
                    var newHighlight = attractionPost.ToHighlight(attraction.Id, createdBy);

                    await context.Highlights.AddAsync(newHighlight);
                    await context.SaveChangesAsync();

                    return ToAttractionViewModel(newHighlight, attraction);
                }
            }
            // if does not exist, create the attraction first
            else
            {
                var newAttraction = attractionPost.ToAttraction();
                await context.Attractions.AddAsync(newAttraction);
                await context.SaveChangesAsync();

                var newDefaultHighlight = attractionPost.ToHighlight(newAttraction.Id);
                await context.Highlights.AddAsync(newDefaultHighlight);

                Highlight highlight;

                if (isDefault)
                {
                    highlight = newDefaultHighlight;
                }
                else
                {
                    highlight = attractionPost.ToHighlight(newAttraction.Id, createdBy);
                    await context.Highlights.AddAsync(highlight);
                }

                await context.SaveChangesAsync();
                return ToAttractionViewModel(highlight, newAttraction);
            }
        }

        /// <summary>
        /// Update a highlight you own
        /// </summary>
        /// <param name="highlight">highlight</param>
        /// <param name="attractionPatch">attraction detail be updated</param>
        /// <returns>the attraction up to date</returns>
        public async Task<AttractionViewModel> PatchHighlightAsync(
            Highlight highlight,
            AttractionPatchViewModel attractionPatch
        )
        {
            // change highlight
            highlight.Description = attractionPatch.Description?.Trim() ?? highlight.Description;
            highlight.LinkId = attractionPatch.LinkId ?? highlight.LinkId;

            await context.SaveChangesAsync();

            return ToAttractionViewModel(highlight);
        }

        /// <summary>
        /// Delete a highlight
        /// </summary>
        /// <param name="highlight">the highlight to be deleted</param>
        /// <returns>the deleted attraction</returns>
        public async Task<AttractionViewModel> DeleteHighlightAsync(Highlight highlight)
        {
            // replace all usage of this attraction with the default attraction
            var defaultHighlight = GetDefaultHighlight(highlight.AttractionId);
            var taos = context.TripAttractionOrders.Where(tao => tao.HighlightId == highlight.Id);

            foreach (var tao in taos)
            {
                tao.HighlightId = defaultHighlight!.Id;
            }

            // delete the attraction
            context.Highlights.Remove(highlight);
            await context.SaveChangesAsync();

            return ToAttractionViewModel(highlight);
        }

        /// <summary>
        /// Check if new attraction's detail is valid
        /// </summary>
        /// <param name="newAttraction">new attraction</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePost(AttractionPostViewModel newAttraction)
        {
            var invalidParams = new List<string>();

            if (newAttraction.Name.Length > 50)
                invalidParams.Add("name");
            if (newAttraction.Description?.Length > 500)
                invalidParams.Add("description");
            if (newAttraction.Address.Length > 200)
                invalidParams.Add("address");

            return invalidParams;
        }

        /// <summary>
        /// Check if attraction's detail is valid
        /// </summary>
        /// <param name="attraction">existing attraction</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidatePatch(AttractionPatchViewModel attraction)
        {
            var invalidParams = new List<string>();

            if (attraction.Description?.Length > 500)
                invalidParams.Add("description");

            return invalidParams;
        }

        public AttractionViewModel ToAttractionViewModel(
            Highlight highlight,
            Attraction? attraction = null
        )
        {
            var _attraction =
                attraction ?? context.Attractions.First(a => a.Id == highlight.AttractionId);
            var attractionViewModel = (AttractionViewModel)_attraction!;

            attractionViewModel.Id = highlight.Id;
            attractionViewModel.Description = highlight.Description;
            attractionViewModel.CreatedBy = highlight.CreatedBy;
            attractionViewModel.LinkId = highlight.LinkId;

            return attractionViewModel;
        }

        private Highlight? GetDefaultHighlight(int attractionId)
        {
            return context.Highlights.FirstOrDefault(h =>
                h.AttractionId == attractionId && h.CreatedBy == null
            );
        }
    }
}
