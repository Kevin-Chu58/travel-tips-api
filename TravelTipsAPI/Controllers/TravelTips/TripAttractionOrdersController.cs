using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

namespace TravelTipsAPI.Controllers.TravelTips
{
    /// <summary>
    /// The controller of Trip Attraction Orders
    /// </summary>
    /// <param name="taosService">trip attraction orders service</param>
    [Route("api/[controller]")]
    public class TripAttractionOrdersController(
        ITripsService tripsService,
        ITripSharesService tripSharesService,
        ITripAttractionOrdersService taosService
    ) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get tao by id
        /// </summary>
        /// <param name="id">tao id</param>
        /// <param name="dayId">day id</param>
        /// <returns>the tao with the id</returns>
        [HttpGet]
        [Route("{id}/day/{dayId}")]
        [AllowAnonymous]
        [SetUserId]
        public async Task<ActionResult<TripAttractionOrderViewModel>> GetTaoById(int id, int dayId)
        {
            // check if the trip is public or the user is the owner or shared user
            var trip = tripsService.FindTripByParams(dayId: dayId);

            if (trip is null)
            {
                return NotFound(Messages.TripNotFound);
            }

            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var isShared = tripSharesService.IsTripSharedWithUser(trip.Id, userId);

            var isRestricted = trip.CreatedBy == userId || isShared;

            if ((!trip.IsPublic && !isRestricted) || trip.IsHidden)
                return BadRequest(Messages.TripUnauthorized);

            try
            {
                var taoViewModel = await taosService.GetTaoById(id, isRestricted, true);
                return Ok(taoViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get a list of tao by day id
        /// </summary>
        /// <param name="dayId">day id</param>
        /// <returns>a list of tao on that day</returns>
        [HttpGet]
        [Route("day/{dayId}")]
        [AllowAnonymous]
        [SetUserId]
        public async Task<ActionResult<IEnumerable<TripAttractionOrderViewModel>>> GetTaosByDayId(
            int dayId
        )
        {
            // check if the trip is public or the user is the owner or shared user
            var trip = tripsService.FindTripByParams(dayId: dayId);

            if (trip is null)
            {
                return NotFound(Messages.TripNotFound);
            }

            var userId = (int)(HttpContext.Items["user_id"] ?? 0);
            var isShared = tripSharesService.IsTripSharedWithUser(trip.Id, userId);

            var isRestricted = trip.CreatedBy == userId || isShared;

            if ((!trip.IsPublic && !isRestricted) || trip.IsHidden)
                return BadRequest(Messages.TripUnauthorized);

            var taoViewModels = await taosService.GetTaosByDayId(dayId, isRestricted, true);
            return Ok(taoViewModels);
        }

        /// <summary>
        /// create a new tao under day id
        /// </summary>
        /// <param name="id">day id</param>
        /// <param name="newTao">new tao</param>
        /// <returns>the new tao</returns>
        [HttpPost]
        [Route("{id}")]
        [IsOwner(Resource = Resources.DAYS)]
        public async Task<ActionResult<TripAttractionOrderViewModel>> CreateNewTao(
            int id,
            [FromBody] TripAttractionOrderPostViewModel newTao
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // check day max restriction
            var taoViewModels = await taosService.GetTaosByDayId(id, true);

            if (taoViewModels.Count() >= NumberConstraints.MAX_TAO_PER_DAY)
                return BadRequest(Messages.TaoMaxReached);

            try
            {
                // check time is valid
                taosService.IsTimeValid(newTao.Start);
                taosService.IsTimeValid(newTao.End);

                // check tao has conflict
                taosService.IsTaoConflicted(newTao.Start, newTao.End, newTao.DayId);

                var taoId = await taosService.PostTao(newTao, userId);
                var tao = await taosService.GetTaoById(taoId, true, true);

                return Ok(tao);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// update an existing tao
        /// </summary>
        /// <param name="id">tao id</param>
        /// <param name="taoPatch">tao details to be updated</param>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<TripAttractionOrderViewModel>> UpdateTao(
            int id,
            [FromBody] TripAttractionOrderPatchViewModel taoPatch
        )
        {
            var tao = taosService.FindTaoById(id);

            if (tao is null)
                return NotFound(Messages.TaoNotFound);

            try
            {
                // check time is valid
                if (taoPatch.Start != null)
                    taosService.IsTimeValid((TimeOnly)taoPatch.Start);

                if (taoPatch.End != null)
                    taosService.IsTimeValid((TimeOnly)taoPatch.End);

                // check tao has conflict
                if (taoPatch.Start != null || taoPatch.End != null)
                {
                    taosService.IsTaoConflicted(
                        taoPatch.Start ?? tao.Start,
                        taoPatch.End ?? tao.End,
                        tao.DayId,
                        id
                    );
                }

                var taoId = await taosService.PatchTao(taoPatch, tao);
                var updatedTao = await taosService.GetTaoById(taoId, true, true);

                return Ok(updatedTao);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Detach highlight from tao
        /// </summary>
        /// <param name="id">tao id</param>
        /// <returns>updated tao</returns>
        [HttpPatch]
        [Route("{id}/detach-highlight")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<TripAttractionOrderViewModel>> UpdateTaoHighlight(int id)
        {
            var tao = taosService.FindTaoById(id);

            if (tao is null)
                return NotFound(Messages.TaoNotFound);

            try
            {
                var taoId = await taosService.PatchTaoDetachHighlight(tao);
                var updatedTao = await taosService.GetTaoById(taoId, true);

                return Ok(updatedTao);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update tao privacy status
        /// </summary>
        /// <param name="id">tao id</param>
        /// <param name="status">privacy status</param>
        /// <returns>updated privacy status</returns>
        [HttpPatch]
        [Route("{id}/privacy/{status}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<bool>> UpdateTaoPrivacy(int id, bool status)
        {
            var tao = taosService.FindTaoById(id);
            if (tao is null)
                return NotFound(Messages.TaoNotFound);
            try
            {
                var newStatus = await taosService.PatchTaoSetPrivate(tao, status);
                return Ok(newStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete tao by its id
        /// </summary>
        /// <param name="id">tao id</param>
        /// <returns>deleted tao id</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.TRIP_ATTRACTION_ORDERS)]
        public async Task<ActionResult<int>> DeleteTaosById(int id)
        {
            var tao = taosService.FindTaoById(id);

            var taoId = await taosService.DeleteTaoById(tao!);

            return Ok(taoId);
        }
    }
}
