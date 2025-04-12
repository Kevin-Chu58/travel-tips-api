using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Controllers
{
    /// <summary>
    /// The controller of Prefer Routes
    /// </summary>
    /// <param name="preferRoutesService">prefer routes service</param>
    [Route("api/[controller]")]
    public class PreferRoutesController(IPreferRoutesService preferRoutesService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get prefer route search results by params
        /// </summary>
        /// <param name="type">prefer route type</param>
        /// <param name="reference">prefer route ref</param>
        /// <param name="departOsmId">prefer route depart osm id</param>
        /// <param name="arrivalOsmId">prefer route arrival osm id</param>
        /// <param name="estimateTimeMin">prefer route min estimate time</param>
        /// <param name="estimateTimeMax">prefer route max estimate time</param>
        /// <param name="isOwner">user owns prefer routes</param>
        /// <param name="time">timestamp</param>
        /// <returns>the prefer route search results that satisfy the search params</returns>
        [HttpGet]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<PreferRouteSearchViewModel> GetPreferRoutesByParams([FromQuery] int? type, string? reference, 
            int? departOsmId, int? arrivalOsmId, int? estimateTimeMin, int? estimateTimeMax, bool? isOwner, int time)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            int? createdBy = isOwner == true ? userId : null;

            var preferRouteViewModels = preferRoutesService.GetPreferRoutesByParams(
                 type,
                 reference, 
                 departOsmId,
                 arrivalOsmId,
                 estimateTimeMin,
                 estimateTimeMax,
                 createdBy
            );

            var preferRouteSearch = new PreferRouteSearchViewModel
            {
                TimeStamp = time,
                PreferRoutes = preferRouteViewModels
            };

            return Ok(preferRouteSearch);
        }

        // TODO - add comments
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<PreferRouteViewModel>> PostNewPreferRouteAsync([FromBody] PreferRoutePostViewModel newPreferRoute)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var preferRouteViewModel = await preferRoutesService.PostPreferRoutesAsync(userId, newPreferRoute);

            return Ok(preferRouteViewModel);
        }

        // TODO - add comments
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.PREFER_ROUTES)]
        public async Task<ActionResult<PreferRouteViewModel>> PatchPreferRouteAsync(int id, [FromBody] PreferRoutePatchViewModel preferRoute)
        {
            var preferRouteViewModel = await preferRoutesService.PatchPreferRoutesAsync(id, preferRoute);

            return Ok(preferRouteViewModel);
        }
    }
}
