//using System.Reflection;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using TravelTipsAPI.Authorization;
//using TravelTipsAPI.Constants;
//using TravelTipsAPI.Models.TravelTipsModels;
//using TravelTipsAPI.ViewModels.db_basic;
//using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

//namespace TravelTipsAPI.Controllers.TravelTips
//{
//    /// <summary>
//    /// The controller of Trip Attraction Orders
//    /// </summary>
//    /// <param name="taosService">trip attraction orders service</param>
//    [Route("api/[controller]")]
//    public class TripAttractionOrdersController(ITripAttractionOrdersService taosService)
//        : TravelTipsControllerBase
//    {
//        // taos

//        /// <summary>
//        /// Get a trip attraction order in a public trip by id
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <returns>the trip attraction order with the id</returns>
//        [HttpGet]
//        [Route("{id}")]
//        [AllowAnonymous]
//        public ActionResult<TripAttractionOrderViewModel> GetPublicTripAttractionOrderById(int id)
//        {
//            try
//            {
//                var tao = taosService.FindTripAttractionOrderById(id, true);

//                return Ok(taosService.ToViewModel(tao));
//            }
//            catch (Exception ex)
//            {
//                return NotFound(ex.Message);
//            }
//        }

//        /// <summary>
//        /// Get a trip attraction order you own by id
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <returns>the trip attraction order with the id</returns>
//        [HttpGet]
//        [Route("my/{id}")]
//        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
//        public ActionResult<TripAttractionOrderViewModel> GetMyTripAttractionOrderById(int id)
//        {
//            var taoViewModel = taosService.FindTripAttractionOrderById(id);

//            return Ok(taoViewModel);
//        }

//        /// <summary>
//        /// Get a list of trip attraction orders you own by day id
//        /// </summary>
//        /// <param name="id">day id</param>
//        /// <returns>a list of trip attraction orders of a day</returns>
//        [HttpGet]
//        [Route("my/day/{id}")]
//        [IsOwner(Resource = Resources.DAYS)]
//        public ActionResult<
//            IEnumerable<TripAttractionOrderViewModel>
//        > GetMyTripAttractionOrdersByDayId(int id)
//        {
//            var taos = taosService.GetTripAttractionOrdersByDayId(id);

//            var taoViewModels = new List<TripAttractionOrderViewModel>();
//            foreach (var tao in taos)
//            {
//                taoViewModels.Add(taosService.ToViewModel(tao));
//            }

//            return Ok(taoViewModels);
//        }

//        /// <summary>
//        /// Create a new trip attraction order
//        /// </summary>
//        /// <param name="newTao">new trip attraction order details</param>
//        /// <returns>the new trip attraction order</returns>
//        [HttpPost]
//        [Route("")]
//        [IsOwner(Resource = Resources.NONE)]
//        public async Task<
//            ActionResult<IEnumerable<TripAttractionOrderViewModel>>
//        > PostNewTripAttractionOrderAsync([FromBody] TripAttractionOrderPostViewModel newTao)
//        {
//            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

//            try
//            {
//                var taoViewModel = await taosService.PostTripAttractionOrderAsync(userId, newTao);
//                var taos = taosService.GetTripAttractionOrdersByDayId(taoViewModel.DayId);
//                return Ok(taos.Select(tao => taosService.ToViewModel(tao)).ToList());
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(ex.Message);
//            }
//        }

//        /// <summary>
//        /// Update an existing trip attraction order
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <param name="taoPatch">the new trip attraction order to be updated</param>
//        /// <returns>the trip attraction order up to date</returns>
//        [HttpPatch]
//        [Route("{id}")]
//        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
//        public async Task<
//            ActionResult<IEnumerable<TripAttractionOrderViewModel>>
//        > PatchTripAttractionOrderAsync(
//            int id,
//            [FromBody] TripAttractionOrderPatchViewModel taoPatch
//        )
//        {
//            var tao = taosService.FindTripAttractionOrderById(id);

//            try
//            {
//                var taoViewModel = await taosService.PatchTripAttractionOrderAsync(tao, taoPatch);

//                // if order is changed, also update the order
//                if (taoPatch.Order != null && taoPatch.Order != tao.Order)
//                {
//                    var taoViewModels = await taosService.SetOrderAsync(tao, (int)taoPatch.Order);
//                    return Ok(taoViewModels);
//                }

//                var taos = taosService.GetTripAttractionOrdersByDayId(taoViewModel.DayId);
//                return Ok(taos.Select(tao => taosService.ToViewModel(tao)).ToList());
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(ex.Message);
//            }
//        }

//        /// <summary>
//        /// Switch a trip attraction order to a new order in the same day
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <param name="newOrder">the new order</param>
//        /// <returns>a list of trip attraction order ids in new order</returns>
//        [HttpPatch]
//        [Route("{id}/order")]
//        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
//        public async Task<ActionResult<IEnumerable<TripAttractionOrderViewModel>>> SetOrderAsync(
//            int id,
//            [FromBody] int newOrder
//        )
//        {
//            var tao = taosService.FindTripAttractionOrderById(id);

//            try
//            {
//                var taoViewModels = await taosService.SetOrderAsync(tao, newOrder);

//                return Ok(taoViewModels);
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(ex.Message);
//            }
//        }

//        /// <summary>
//        /// Delete a trip attraction order you own
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <returns>the trip attraction order deleted</returns>
//        [HttpDelete]
//        [Route("{id}")]
//        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
//        public async Task<
//            ActionResult<TripAttractionOrderViewModel>
//        > DeleteTripAttractionOrderAsync(int id)
//        {
//            var tao = taosService.FindTripAttractionOrderById(id);
//            var deletedTaoViewModel = await taosService.DeleteTripAttractionOrderAsync(tao);

//            return Ok(deletedTaoViewModel);
//        }

//        // taors

//        /// <summary>
//        /// Create a new trip attraction order route
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <param name="routeId">prefer route id</param>
//        /// <param name="order">new order</param>
//        /// <returns>the trip attraction order where the new trip attraction order route is</returns>
//        [HttpPost]
//        [Route("{id}/routes/{routeId}")]
//        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
//        public async Task<
//            ActionResult<TripAttractionOrderViewModel>
//        > PostNewTripAttractionOrderRouteAsync(int id, int routeId, [FromBody] int order)
//        {
//            try
//            {
//                var tao = taosService.FindTripAttractionOrderRoute(id, routeId);

//                if (tao != null)
//                {
//                    return BadRequest(Messages.TaorExist);
//                }
//            }
//            catch (Exception) { }

//            try
//            {
//                var taoViewModel = await taosService.PostNewTripAttractionOrderRouteAsync(
//                    id,
//                    routeId,
//                    order
//                );

//                return Ok(taoViewModel);
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(ex.Message);
//            }
//        }

//        /// <summary>
//        /// Update a trip attraction order route you own
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <param name="routeId">prefer route id</param>
//        /// <param name="order">new order</param>
//        /// <returns>the trip attraction order where the new trip attraction order route is</returns>
//        [HttpPatch]
//        [Route("{id}/routes/{routeId}")]
//        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
//        public async Task<
//            ActionResult<IEnumerable<TripAttractionOrderViewModel>>
//        > SetPreferRouteOrderAsync(int id, int routeId, [FromBody] int order)
//        {
//            // check if taor exists
//            TripAttractionOrderRoute taor;
//            try
//            {
//                taor = taosService.FindTripAttractionOrderRoute(id, routeId);
//            }
//            catch (Exception ex)
//            {
//                return NotFound(ex.Message);
//            }

//            try
//            {
//                var taoViewModel = await taosService.SetPreferRouteOrderAsync(taor, order);

//                return Ok(taoViewModel);
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(ex.Message);
//            }
//        }

//        /// <summary>
//        /// Delete a trip attraction order route you own
//        /// </summary>
//        /// <param name="id">trip attraction order id</param>
//        /// <param name="routeId">prefer route id</param>
//        /// <returns>the trip attraction order where the new trip attraction order route was</returns>
//        [HttpDelete]
//        [Route("{id}/routes/{routeId}")]
//        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
//        public async Task<
//            ActionResult<TripAttractionOrderViewModel>
//        > DeleteTripAttractionOrderRouteAsync(int id, int routeId)
//        {
//            // check if taor exists
//            TripAttractionOrderRoute taor;
//            try
//            {
//                taor = taosService.FindTripAttractionOrderRoute(id, routeId);
//            }
//            catch (Exception ex)
//            {
//                return NotFound(ex.Message);
//            }

//            var taoViewModel = await taosService.DeleteTripAttractionOrderRouteAsync(taor);

//            return Ok(taoViewModel);
//        }
//    }
//}
