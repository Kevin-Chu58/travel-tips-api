using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Controllers.TravelTips;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.HereMapServices.HereMapSchema;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Controllers.HereMap
{
    /// <summary>
    /// The controller of Here Map API
    /// </summary>
    /// <param name="hereMapDiscoverService">here map discover service</param>
    [Route("api/[controller]")]
    public class HereMapController(
        IHereMapDiscoverService hereMapDiscoverService,
        IHereMapLookupService hereMapLookupService,
        IHereMapRoutingService hereMapRoutingService,
        ITripAttractionOrdersService tripAttractionOrdersService
    ) : TravelTipsControllerBase
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
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("routing/{dayId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<HereRoutingResponse>>> SearchRoutingAsync(
            int dayId
        )
        {
            try
            {
                var attractionRoutings = tripAttractionOrdersService.GetAttractionRoutingsByDayId(
                    dayId
                );

                var routes = new List<HereRoutingResponse>();

                for (var i = 1; i < attractionRoutings.Count; i++)
                {
                    var prevRouting = attractionRoutings[i - 1];
                    var curRouting = attractionRoutings[i];

                    var hereRoutingResponse = await hereMapRoutingService.GetRouteAsync(
                        curRouting.TransportMode,
                        prevRouting.Position.Lat,
                        prevRouting.Position.Lng,
                        curRouting.Position.Lat,
                        curRouting.Position.Lng
                    );

                    routes.Add(hereRoutingResponse);
                }

                return Ok(routes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
