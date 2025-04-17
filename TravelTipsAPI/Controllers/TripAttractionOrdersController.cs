using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Trip Attraction Orders
    /// </summary>
    /// <param name="taosService">trip attraction orders service</param>
    [Route("api/[controller]")]
    public class TripAttractionOrdersController(ITripAttractionOrdersService taosService) : TravelTipsControllerBase
    {
        // taos

        /// <summary>
        /// Get a trip attraction order in a public trip by id
        /// </summary>
        /// <param name="id">trip attraction order id</param>
        /// <returns>the trip attraction order with the id</returns>
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public ActionResult<TripAttractionOrderViewModel> GetPublicTripAttractionOrderById(int id)
        {
            var taoViewModel = taosService
                .GetTripAttractionOrderById(id, true);

            return Ok(taoViewModel);
        }

        /// <summary>
        /// Get a trip attraction order you own by id
        /// </summary>
        /// <param name="id">trip attraction order</param>
        /// <returns>the trip attraction order with the id</returns>
        [HttpGet]
        [Route("my/{id}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public ActionResult<TripAttractionOrderViewModel> GetYourTripAttractionOrderById(int id)
        {
            var taoViewModel = taosService
                .GetTripAttractionOrderById(id, null);

            return Ok(taoViewModel);
        }

        /// <summary>
        /// Create a new trip attraction order
        /// </summary>
        /// <param name="newTao">new trip attraction order details</param>
        /// <returns>the new trip attraction order</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<TripAttractionOrderViewModel>> PostNewTripAttractionOrderAsync(
            [FromBody] TripAttractionOrderPostViewModel newTao)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var taoViewModel = await taosService.PostTripAttractionOrderAsync(userId, newTao);

            return Ok(taoViewModel);
        }

        /// <summary>
        /// Update an existing trip attraction order
        /// </summary>
        /// <param name="id">trip attraction order id</param>
        /// <param name="tao">the new trip attraction order to be updated</param>
        /// <returns>the trip attraction order up to date</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<TripAttractionOrderViewModel>> PatchTripAttractionOrderAsync(
            int id, [FromBody] TripAttractionOrderPatchViewModel tao)
        {
            var taoViewModel = await taosService.PatchTripAttractionOrderAsync(id, tao);

            return Ok(taoViewModel);
        }

        /// <summary>
        /// Switch a trip attraction order to a new order in the same day
        /// </summary>
        /// <param name="id">trip attraction order id</param>
        /// <param name="newOrder">the new order</param>
        /// <returns>a list of trip attraction order ids in new order</returns>
        [HttpPatch]
        [Route("{id}/switch")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<IEnumerable<TripAttractionOrderViewModel>>> SetOrderAsync(int id, [FromBody] int newOrder)
        {
            var taoViewModels = await taosService.SetOrderAsync(id, newOrder);

            return Ok(taoViewModels);
        }

        /// <summary>
        /// Delete a trip attraction order you own
        /// </summary>
        /// <param name="id">trip attraction order id</param>
        /// <returns>the trip attraction order deleted</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<TripAttractionOrderViewModel>> DeleteTripAttractionOrderAsync(int id)
        {
            var deletedTaoViewModel = await taosService.DeleteTripAttractionOrderAsync(id);

            return Ok(deletedTaoViewModel);
        }

        // taors

        /// <summary>
        /// Create a new trip attraction order route
        /// </summary>
        /// <param name="id">trip attraction order id</param>
        /// <param name="routeId">prefer route id</param>
        /// <param name="order">new order</param>
        /// <returns>the trip attraction order where the new trip attraction order route is</returns>
        [HttpPost]
        [Route("{id}/routes/{routeId}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<TripAttractionOrderViewModel>> PostNewTripAttractionOrderRouteAsync(
            int id, int routeId, [FromBody] int order)
        {
            var taoViewModel = await taosService.PostNewTripAttractionOrderRouteAsync(id, routeId, order);

            return Ok(taoViewModel);
        }

        /// <summary>
        /// Update a trip attraction order route you own
        /// </summary>
        /// <param name="id">trip attraction order id</param>
        /// <param name="routeId">prefer route id</param>
        /// <param name="order">new order</param>
        /// <returns>the trip attraction order where the new trip attraction order route is</returns>
        [HttpPatch]
        [Route("{id}/routes/{routeId}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<IEnumerable<TripAttractionOrderViewModel>>> SetPreferRouteOrderAsync(
            int id, int routeId, [FromBody] int order)
        {
            var taoViewModel = await taosService.SetPreferRouteOrderAsync(id, routeId, order);

            return Ok(taoViewModel);
        }

        /// <summary>
        /// Delete a trip attraction order route you own
        /// </summary>
        /// <param name="id">trip attraction order id</param>
        /// <param name="routeId">prefer route id</param>
        /// <returns>the trip attraction order where the new trip attraction order route was</returns>
        [HttpDelete]
        [Route("{id}/routes/{routeId}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<TripAttractionOrderViewModel>>  DeleteTripAttractionOrderRouteAsync(
            int id, int routeId)
        {
            var taoViewModel = await taosService.DeleteTripAttractionOrderRouteAsync(id, routeId);

            return Ok(taoViewModel);
        }
    }
}
