using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Services.TravelTipsServices.Search
{
    /// <summary>
    /// The Service of Regions
    /// </summary>
    public class RegionsService : IRegionsService
    {
        // In your service or singleton
        private readonly IReadOnlyDictionary<int, RegionViewModel> _regionCache;
        private readonly TravelTipsContext _context;

        /// <summary>
        /// Constructor for RegionsService
        /// </summary>
        /// <param name="context">TravelTipsContext instance</param>
        public RegionsService(TravelTipsContext context)
        {
            _context = context;
            _regionCache = _context
                .Regions.AsNoTracking()
                .Select(r => (RegionViewModel)r)
                .ToDictionary(r => r.Id);
        }

        /// <summary>
        /// Get region by id
        /// </summary>
        /// <param name="id">id of the region</param>
        /// <returns>the region with the id</returns>
        public RegionViewModel GetRegionById(int id)
        {
            _regionCache.TryGetValue(id, out var region);
            if (region is null)
                throw new Exception(Messages.RegionNotFound);
            return region;
        }

        /// <summary>
        /// Get region by name
        /// </summary>
        /// <param name="name">region name</param>
        /// <returns>region with the name</returns>
        public RegionViewModel GetRegionByName(string name)
        {
            var region = _regionCache.Values.FirstOrDefault(r => r.Name == name);
            if (region is null)
                throw new Exception(Messages.RegionNotFound);
            return region;
        }

        /// <summary>
        /// Get regions by params
        /// </summary>
        /// <param name="type">region type</param>
        /// <param name="parentRegionId">parent region id</param>
        /// <returns>a list of regions</returns>
        public IEnumerable<RegionViewModel> GetRegionsByParams(
            string type,
            string? name = null,
            int? parentRegionId = null
        )
        {
            var regions = _regionCache.Values.Where(region => region.Type == type);

            if (parentRegionId != null)
            {
                regions = regions.Where(region => region.ParentRegionId == parentRegionId);
            }

            if (name != null)
            {
                regions = regions.Where(region => region.Name.StartsWith(name));
            }

            return regions;
        }

        /// <summary>
        /// Get a region by country slug and state slug
        /// </summary>
        /// <param name="countrySlug">country slug</param>
        /// <param name="stateSlug">state slug</param>
        /// <returns>the region</returns>
        public RegionViewModel GetRegionByCountryAndState(string countrySlug, string? stateSlug)
        {
            var countryRegion = _regionCache.Values.FirstOrDefault(r =>
                r.Type == "Country" && r.Slug == countrySlug
            );

            if (countryRegion is null)
                throw new Exception(Messages.RegionNotFound);

            if (string.IsNullOrEmpty(stateSlug))
                return countryRegion;

            var stateRegion = _regionCache.Values.FirstOrDefault(r =>
                r.Type == "State" && r.Slug == stateSlug && r.ParentRegionId == countryRegion.Id
            );

            if (stateRegion is null)
                throw new Exception(Messages.RegionNotFound);

            return stateRegion;
        }

        /// <summary>
        /// Build a complete region by leaf region id
        /// </summary>
        /// <param name="regionId">leaf region id</param>
        /// <returns>a complete region built from the leaf region</returns>
        public RegionCompleteViewModel BuildRegionComplete(int regionId)
        {
            var region = GetRegionById(regionId);

            var result = new RegionCompleteViewModel();

            // Walk up the tree until Continent
            while (region != null)
            {
                switch (region.Type)
                {
                    case "Continent":
                        result.Continent = region;
                        break;
                    case "Country":
                        result.Country = region;
                        break;
                    case "State":
                        result.State = region;
                        break;
                    case "Area":
                        result.Area = region;
                        break;
                }

                if (region.ParentRegionId.HasValue)
                {
                    _regionCache.TryGetValue(region.ParentRegionId.Value, out region);
                }
                else
                {
                    break;
                }
            }

            // Validation: ensure required regions exist
            //if (result.Continent == null || result.Country == null)
            //    throw new InvalidOperationException(Messages.RegionRootInvalid);

            return result;
        }
    }
}
