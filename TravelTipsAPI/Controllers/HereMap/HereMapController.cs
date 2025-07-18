using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;

namespace TravelTipsAPI.Controllers.HereMap
{
    /// <summary>
    /// The controller of Here Map API
    /// </summary>
    /// <param name="hereMapDiscoverService">here map discover service</param>
    [Route("api/[controller]")]
    public class HereMapController(IHereMapDiscoverService hereMapDiscoverService)
        : TravelTipsControllerBase
    {
        /// <summary>
        /// Find a list of HerePlace by query name
        /// </summary>
        /// <param name="query">search name</param>
        /// <param name="lat">lat to search from</param>
        /// <param name="lng">lng to search from</param>
        /// <param name="limit">returned number of items</param>
        /// <returns>a list of HerePlace</returns>
        [HttpGet]
        [Route("discover")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Attraction2ViewModel>>> SearchPlaceByNameAsync(
            [FromQuery] string query,
            decimal lat,
            decimal lng,
            int? limit
        )
        {
            try
            {
                var attractions = await hereMapDiscoverService.SearchPlaceByNameAsync(
                    query,
                    lat,
                    lng,
                    limit
                );
                return Ok(attractions);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
