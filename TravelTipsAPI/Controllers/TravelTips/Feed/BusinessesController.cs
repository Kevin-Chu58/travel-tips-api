using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class BusinessesController(IBusinessesService businessesService)
        : TravelTipsControllerBase
    {
        /// <summary>
        /// Get all my businesses
        /// </summary>
        /// <returns>a list of my businesses</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<IEnumerable<BusinessViewModel>> GetMyBusinesses()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var businesses = businessesService.GetBusinessesByParams(userId, null);
            return Ok(businesses);
        }

        /// <summary>
        /// Get businesses by params
        /// </summary>
        /// <returns>a list of businesses fits the params</returns>
        [HttpGet]
        [Route("")]
        [HasRole(Role = UserRoles.REVIEWER)]
        public ActionResult<IEnumerable<BusinessViewModel>> GetBusinessesByParams(
            [FromQuery] int? userId,
            [FromQuery] AdStatus? status
        )
        {
            var businesses = businessesService.GetBusinessesByParams(userId, status);
            return Ok(businesses);
        }

        /// <summary>
        /// Create new business in pending status
        /// </summary>
        /// <param name="newBusiness">new business</param>
        /// <returns>the newly created business</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<BusinessViewModel>> PostNewBusiness(
            [FromBody] BusinessPostViewModel newBusiness
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var business = await businessesService.PostNewBusiness(newBusiness, userId);
            return Ok(business);
        }

        /// <summary>
        /// Update an existing business details
        /// </summary>
        /// <param name="id">business id</param>
        /// <param name="updatedBusiness">business details to be updated</param>
        /// <returns>the updated business</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.BUSINESSES)]
        public async Task<ActionResult<BusinessViewModel>> UpdateBusiness(
            int id,
            [FromBody] BusinessPatchViewModel updatedBusiness
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var business = businessesService.FindBusinessById(id);
            if (business == null)
                return NotFound(Messages.BusinessNotFound);

            var result = await businessesService.UpdateBusiness(business, updatedBusiness);
            return Ok(result);
        }

        /// <summary>
        /// Update a business active status only when business is either active or inactive
        /// </summary>
        /// <param name="id">business id</param>
        /// <param name="isActive">new active status</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/active-status")]
        [IsOwner(Resource = Resources.BUSINESSES)]
        public async Task<ActionResult<string>> UpdateBusinessActiveStatus(
            int id,
            [FromQuery] bool isActive
        )
        {
            try
            {
                var business = businessesService.FindBusinessById(id);
                if (business == null)
                    return NotFound(Messages.BusinessNotFound);

                var newStatus = await businessesService.UpdateBusinessActiveStatus(
                    business,
                    isActive
                );
                return Ok(newStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing business status
        /// </summary>
        /// <param name="id">business id</param>
        /// <param name="status">new status</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/status")]
        [HasRole(Role = UserRoles.REVIEWER)]
        public async Task<ActionResult<string>> UpdateBusinessStatus(int id, [FromQuery] int status)
        {
            try
            {
                var business = businessesService.FindBusinessById(id);
                if (business == null)
                    return NotFound(Messages.BusinessNotFound);

                var newStatus = await businessesService.UpdateBusinessStatus(
                    business,
                    (AdStatus)status
                );
                return Ok(newStatus);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
