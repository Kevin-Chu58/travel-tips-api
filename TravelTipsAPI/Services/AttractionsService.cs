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
        /// Get an attraction by its id
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <returns>the attraction with this id</returns>
        public Attraction GetAttractionById(int id)
        {
            var attraction = context.Attractions.Find(id);

            if (attraction == null)
                throw new FileNotFoundException(Messages.AttractionNotFound);

            return attraction;
        }

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
        /// <param name="osmType">osm type</param>
        /// <param name="ownerId">user id</param>
        /// <returns>a list of highlights that satisfy the search params</returns>
        public IEnumerable<AttractionViewModel> GetHighlightsByParams(
            string? name,
            long? osmId,
            string? osmType,
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
            if (osmType != null)
                attractions = attractions.Where(a => a.OsmType == osmType);

            foreach (var attraction in attractions)
            {
                var highlights = context.Highlights.Where(h => h.AttractionId == attraction.Id);

                // filter by the createdBy param
                if (ownerId != null)
                    highlights = highlights.Where(h => h.CreatedBy == ownerId);

                var viewModels = new List<AttractionViewModel>();

                foreach (var highlight in highlights)
                {
                    viewModels.Add(ToAttractionViewModel(highlight, attraction));
                }

                attractionViewModels = [.. attractionViewModels, .. viewModels];
            }

            return attractionViewModels;
        }

        /// <summary>
        /// Get attraction highlights by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of attraction highlights owned by the user</returns>
        public IEnumerable<AttractionHighlightsViewModel> GetAttractionHighlightsByUserId(int id)
        {
            var highlightViewModels = context
                .Highlights.Where(h => h.CreatedBy == id)
                .Select(h => (HighlightViewModel)h)
                .ToList();

            var attractionIds = highlightViewModels.Select(h => h.AttractionId).Distinct().ToList();

            var ahViewModels = context
                .Attractions.Where(a => attractionIds.Contains(a.Id))
                .Select(a => (AttractionHighlightsViewModel)a)
                .ToList();

            foreach (var ahViewModel in ahViewModels)
            {
                ahViewModel.Highlights =
                [
                    .. highlightViewModels.Where(h => h.AttractionId == ahViewModel.Id),
                ];
            }

            return ahViewModels;
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

        public async Task<AttractionViewModel> PostNewAttractionAsync(
            AttractionViewModel attraction
        )
        {
            var newAttraction = (Attraction)attraction;

            await context.Attractions.AddAsync(newAttraction);
            await context.SaveChangesAsync();

            return (AttractionViewModel)newAttraction;
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
                // check if attraction has changed
                var attractionViewModel = (AttractionViewModel)attractionPost;
                var isDeprecated = HasAttractionChanged(attraction, attractionViewModel);

                if (isDeprecated)
                {
                    PatchAttractionAsync(attraction, attractionViewModel);
                    await PatchHighlightsDeprecated(attraction.Id);
                }

                // if attraction exists but default highlight does not, create new default highlight
                var defaultHighlight = GetDefaultHighlight(attraction.Id);
                if (defaultHighlight is null)
                {
                    defaultHighlight = attractionPost.ToHighlight(attraction.Id);

                    await context.Highlights.AddAsync(defaultHighlight);
                    await context.SaveChangesAsync();
                }

                // if is default, return default highlight
                if (isDefault)
                {
                    return ToAttractionViewModel(defaultHighlight, attraction);
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
        /// update the attraction information, does not save upon changes yet!
        /// </summary>
        /// <param name="attraction">attraction to be updated</param>
        /// <param name="attractionViewModel">updated attraction info</param>
        /// <returns>the updated attraction</returns>
        public void PatchAttractionAsync(
            Attraction attraction,
            AttractionViewModel attractionViewModel
        )
        {
            attraction.Lng = attractionViewModel.Lng;
            attraction.Lat = attractionViewModel.Lat;
            attraction.Name = attractionViewModel.Name;
            attraction.Address = attractionViewModel.Address;
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
            // check if attraction has changed
            var attraction = highlight.Attraction;
            var attractionViewModel = (AttractionViewModel)attractionPatch;
            var isDeprecated = HasAttractionChanged(attraction, attractionViewModel);

            if (isDeprecated)
            {
                PatchAttractionAsync(attraction, attractionViewModel);
                await PatchHighlightsDeprecated(attraction.Id);
            }

            // change highlight
            highlight.Description = attractionPatch.Description?.Trim() ?? highlight.Description;
            highlight.LinkId = attractionPatch.LinkId ?? highlight.LinkId;
            highlight.IsDeprecated = false;

            await context.SaveChangesAsync();

            return ToAttractionViewModel(highlight);
        }

        /// <summary>
        /// Mark all the highlights of a certain attraction to deprecated
        /// </summary>
        /// <param name="attractionId">attraction id</param>
        /// <returns>the number of highlights marked as deprecated</returns>
        public async Task<int> PatchHighlightsDeprecated(int attractionId)
        {
            var highlights = context.Highlights.Where(h =>
                h.AttractionId == attractionId && h.IsDeprecated == false
            );

            foreach (var highlight in highlights)
            {
                highlight.IsDeprecated = true;
            }

            await context.SaveChangesAsync();

            return highlights.Count();
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

            // highlights
            attractionViewModel.Id = highlight.Id;
            attractionViewModel.IsDeprecated = highlight.IsDeprecated;
            attractionViewModel.Description = highlight.Description;
            attractionViewModel.CreatedBy = highlight.CreatedBy;
            attractionViewModel.LinkId = highlight.LinkId;

            return attractionViewModel;
        }

        public bool HasAttractionChanged(
            Attraction attraction,
            AttractionViewModel attractionViewModel
        )
        {
            return attraction.Lng != attractionViewModel.Lng
                || attraction.Lat != attractionViewModel.Lat
                || attraction.Name != attractionViewModel.Name
                || attraction.Address != attractionViewModel.Address;
        }

        private Highlight? GetDefaultHighlight(int attractionId)
        {
            return context.Highlights.FirstOrDefault(h =>
                h.AttractionId == attractionId && h.CreatedBy == null
            );
        }
    }
}
