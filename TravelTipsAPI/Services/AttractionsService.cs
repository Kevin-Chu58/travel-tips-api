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
        /// Find an attraction by id
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <returns>the attraction with the id</returns>
        public Attraction FindAttractionById(int id)
        {
            var attraction = context.Attractions.Find(id);

            if (attraction == null)
                throw new Exception(Messages.AttractionNotFound);

            return attraction;
        }

        /// <summary>
        /// Get a list of attractions by search params
        /// </summary>
        /// <param name="name">name to search</param>
        /// <param name="osmId">osm id</param>
        /// <param name="ownerId">user id</param>
        /// <returns>a list of attractions that satisfy the search params</returns>
        public IEnumerable<AttractionViewModel> GetAttractionsByParams(
            string? name,
            int? osmId,
            int? ownerId
        )
        {
            name = name?.Trim().ToLower();

            var attractionViewModels = new List<AttractionViewModel>();

            IEnumerable<Attraction> attractions = context.Attractions.ToList();

            if (name != null && name.Length < SearchConstraints.ATTRACTION_SEARCH_MIN_LENGTH)
            {
                attractions = [];
            }
            else
            {
                if (name != null)
                    attractions = attractions.Where(a => a.Name.ToLower().Contains(name));
            }

            if (ownerId != null)
                attractions = attractions.Where(a => a.CreatedBy == ownerId);
            if (osmId != null)
                attractions = attractions.Where(a => a.OsmId == osmId);

            attractionViewModels = attractions.Select(a => (AttractionViewModel)a).ToList();

            return attractionViewModels;
        }

        /// <summary>
        /// Get my attraction ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of my attraction ids</returns>
        public IEnumerable<int> GetMyAttractions(int id)
        {
            var myAttractionIds = context
                .Attractions.Where(a => a.CreatedBy == id)
                .Select(a => a.Id)
                .ToList();

            return myAttractionIds;
        }

        /// <summary>
        /// Create a new attraction
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="attractionPost">new attraction</param>
        /// <returns>the new attraction created</returns>
        public async Task<AttractionViewModel> PostNewAttractionAsync(
            int createdBy,
            AttractionPostViewModel attractionPost
        )
        {
            var newAttraction = attractionPost.ToAttraction(createdBy);

            await context.Attractions.AddAsync(newAttraction);
            await context.SaveChangesAsync();

            return (AttractionViewModel)newAttraction;
        }

        /// <summary>
        /// Update an attraction you own
        /// </summary>
        /// <param name="attraction">attraction</param>
        /// <param name="attractionPatch">attraction detail be updated</param>
        /// <returns>the attraction up to date</returns>
        public async Task<AttractionViewModel> PatchAttractionAsync(
            Attraction attraction,
            AttractionPatchViewModel attractionPatch
        )
        {
            attraction.Name = attractionPatch.Name?.Trim() ?? attraction.Name;
            attraction.Description = attractionPatch.Description?.Trim() ?? attraction.Description;
            attraction.Address = attractionPatch.Address?.Trim() ?? attraction.Address;
            attraction.OsmId = attractionPatch.OsmId ?? attraction.OsmId;
            attraction.LinkId = attractionPatch.LinkId ?? attraction.LinkId;

            await context.SaveChangesAsync();

            return (AttractionViewModel)attraction;
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
            if (newAttraction.Address.Length > 100)
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

            if (attraction.Name?.Length > 50)
                invalidParams.Add("name");
            if (attraction.Description?.Length > 500)
                invalidParams.Add("description");
            if (attraction.Address?.Length > 100)
                invalidParams.Add("address");

            return invalidParams;
        }
    }
}
