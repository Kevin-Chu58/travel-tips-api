using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.Utils;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
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
        public Attraction FindAttractionById(int id)
        {
            var attraction = context.Attractions.Find(id);

            if (attraction == null)
                throw new FileNotFoundException(Messages.AttractionNotFound);

            return attraction;
        }

        /// <summary>
        /// Get a list of attractions by search params
        /// </summary>
        /// <param name="title">title to search</param>
        /// <param name="ownerId">user id</param>
        /// <returns>a list of highlights that satisfy the search params</returns>
        public IEnumerable<AttractionViewModel> GetAttractionsByParams(
            string? title,
            string? Category = null,
            string? ResultType = null,
            string? HereId = null,
            string? City = null,
            string? State = null,
            string? Country = null,
            int? ownerId = null
        )
        {
            var query = context.Attractions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim().ToLower();
                if (title.Length < SearchConstraints.ATTRACTION_SEARCH_MIN_LENGTH)
                {
                    return [];
                }
                query = query.Where(a => a.Title.ToLower().Contains(title));
            }
            if (!string.IsNullOrWhiteSpace(Category))
            {
                Category = Category.Trim().ToLower();
                query = query.Where(a => a.Category != null && a.Category.ToLower() == Category);
            }
            if (!string.IsNullOrWhiteSpace(ResultType))
            {
                ResultType = ResultType.Trim().ToLower();
                query = query.Where(a => a.ResultType.ToLower() == ResultType);
            }
            if (!string.IsNullOrWhiteSpace(HereId))
            {
                HereId = HereId.Trim();
                query = query.Where(a => a.HereId == HereId);
            }
            if (!string.IsNullOrWhiteSpace(City))
            {
                City = City.Trim().ToLower();
                query = query.Where(a => a.City != null && a.City.ToLower() == City);
            }
            if (!string.IsNullOrWhiteSpace(State))
            {
                State = State.Trim().ToLower();
                query = query.Where(a => a.State != null && a.State.ToLower() == State);
            }
            if (!string.IsNullOrWhiteSpace(Country))
            {
                Country = Country.Trim().ToLower();
                query = query.Where(a => a.Country != null && a.Country.ToLower() == Country);
            }

            var attractions = query.ToList();

            var attractionViewModels = attractions.Select(a => (AttractionViewModel)a).ToList();
            if (ownerId != null)
            {
                // get the number of highlights of each attraction
                foreach (var attraction in attractionViewModels)
                {
                    var highlights = context.Highlights.Where(h => h.AttractionId == attraction.Id);
                    int numHighlights;

                    numHighlights = highlights.Where(h => h.CreatedBy == ownerId).Count();

                    attraction.NumHighlights = numHighlights;
                }

                // if looking for highlights written by a particular user,
                // filter out all attractions with no highlights
                attractionViewModels = [.. attractionViewModels.Where(a => a.NumHighlights > 0)];
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
        /// Create a new attraction
        /// </summary>
        /// <param name="newAttraction">new attraction</param>
        /// <returns>the new attraction</returns>
        public async Task<Attraction> PostNewAttractionAsync(Attraction newAttraction)
        {
            await context.Attractions.AddAsync(newAttraction);
            await context.SaveChangesAsync();

            return newAttraction;
        }

        /// <summary>
        /// Update attraction hereId, Lat & Lng by the updated attraction info in HereMap
        /// </summary>
        /// <param name="attraction">old attraction</param>
        /// <param name="newAttraction">updated attraction</param>
        /// <returns>the updated old attraction</returns>
        public async Task<Attraction> UpdateAttractionAsync(
            Attraction attraction,
            Attraction newAttraction
        )
        {
            attraction.HereId = newAttraction.HereId;
            attraction.Lat = newAttraction.Lat;
            attraction.Lng = newAttraction.Lng;
            attraction.Address = newAttraction.Address;

            await context.SaveChangesAsync();

            return attraction;
        }
    }
}
