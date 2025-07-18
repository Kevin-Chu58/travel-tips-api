using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.ViewModels.nominatim;
using static TravelTipsAPI.Services.NominatimServices.NominatimSchema;

namespace TravelTipsAPI.Controllers.Nominatim
{
    /// <summary>
    /// The controller of Nominatim API
    /// </summary>
    /// <param name="nominatimService"></param>
    [Route("api/[controller]")]
    public class NominatimController(INominatimService nominatimService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get a list of osm entities by search name
        /// </summary>
        /// <param name="search">search name</param>
        /// <returns>a list of osm entities</returns>
        [HttpGet]
        [Route("{search}")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<IEnumerable<OsmEntity>>> GetOsmEntitiesByNameAsync(
            string search
        )
        {
            var osmEntities = await nominatimService.GetOsmEntitiesByNameAsync(search);
            return Ok(osmEntities);
        }
    }
}
