using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
        /// Get a list of attractions by search params
        /// </summary>
        /// <param name="name">name to search</param>
        /// <param name="osmId">osm id</param>
        /// <param name="isPublic">trip public status</param>
        /// <param name="ownerId">user id</param>
        /// <returns>a list of attractions that satisfy the search params</returns>
        public IEnumerable<AttractionViewModel> GetAttractionsByParams(string? name, int? osmId, bool? isPublic, int? ownerId)
        {
            name = name?.Trim().ToLower();

            var attractionViewModels = new List<AttractionViewModel>();

            IEnumerable<Attraction> attractions = [.. context.Attractions];

            if (isPublic != null)
                attractions = context.Attractions
                    .Where(a => a.Trips.Any(t => t.IsPublic == isPublic));

            if (ownerId != null) attractions = attractions.Where(a => a.CreatedBy == ownerId);

            if (name != null) attractions = attractions.Where(a => a.Name.Contains(name));
            if (osmId != null) attractions = attractions.Where(a => a.OsmId == osmId);

            attractionViewModels = attractions.Select(a => (AttractionViewModel)a).ToList();

            return attractionViewModels;
        }

        /// <summary>
        /// Get your attraction ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of your attraction ids</returns>
        public IEnumerable<int> GetYourAttractions(int id)
        {
            var yourAttractionIds = context.Attractions
                .Where(a => a.CreatedBy == id)
                .Select(a => a.Id)
                .ToList();

            return yourAttractionIds;
        }

        /// <summary>
        /// Create a new attraction
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="attractionPost">new attraction</param>
        /// <returns>the new attraction created</returns>
        public async Task<AttractionViewModel> PostNewAttractionAsync(int createdBy, AttractionPostViewModel attractionPost)
        {
            var newAttraction = attractionPost.ToAttraction(createdBy);

            await context.Attractions.AddAsync(newAttraction);
            await context.SaveChangesAsync();

            return (AttractionViewModel)newAttraction;
        }

        /// <summary>
        /// Update an attraction you own
        /// </summary>
        /// <param name="id">attraction id</param>
        /// <param name="attractionPatch">attraction detail be updated</param>
        /// <returns>the attraction up to date</returns>
        public async Task<AttractionViewModel> PatchAttractionAsync(int id, AttractionPatchViewModel attractionPatch)
        {
            var attraction = context.Attractions.Find(id) ?? throw new Exception("Attraction not found.");

            attraction.Name = attractionPatch.Name ?? attraction.Name;
            attraction.Description = attractionPatch.Description ?? attraction.Description;
            attraction.Address = attractionPatch.Address ?? attraction.Address;
            attraction.OsmId = attractionPatch.OsmId ?? attraction.OsmId;
            attraction.LinkId = attractionPatch.LinkId ?? attraction.LinkId;


            await context.SaveChangesAsync();

            return (AttractionViewModel)attraction;
        }
    }
}
