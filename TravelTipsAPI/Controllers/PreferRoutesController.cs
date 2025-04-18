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
    /// The controller of Prefer Routes
    /// </summary>
    /// <param name="preferRoutesService">prefer routes service</param>
    [Route("api/[controller]")]
    public class PreferRoutesController(IPreferRoutesService preferRoutesService) : TravelTipsControllerBase
    {
        // prefer routes

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

        /// <summary>
        /// Create a new prefer route
        /// </summary>
        /// <param name="newPreferRoute">the new prefer route details</param>
        /// <returns>the new prefer route</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<PreferRouteViewModel>> PostNewPreferRouteAsync([FromBody] PreferRoutePostViewModel newPreferRoute)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var preferRouteViewModel = await preferRoutesService.PostPreferRoutesAsync(userId, newPreferRoute);

            return Ok(preferRouteViewModel);
        }

        /// <summary>
        /// Update an existing prefer route
        /// </summary>
        /// <param name="id">prefer route id</param>
        /// <param name="preferRoute">prefer route details to be updated</param>
        /// <returns>the updated prefer route</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.PREFER_ROUTES)]
        public async Task<ActionResult<PreferRouteViewModel>> PatchPreferRouteAsync(int id, [FromBody] PreferRoutePatchViewModel preferRoute)
        {
            var preferRouteViewModel = await preferRoutesService.PatchPreferRoutesAsync(id, preferRoute);

            return Ok(preferRouteViewModel);
        }

        // route types

        /// <summary>
        /// Get all route types of prefer routes
        /// </summary>
        /// <returns>A list of all route types</returns>
        [HttpGet]
        [Route("types")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<RouteTypeViewModel>> GetAllRouteTypes()
        {
            var routeTypeViewModels = preferRoutesService.GetAllRouteTypes();

            return Ok(routeTypeViewModels);
        }

        /// <summary>
        /// Create a new route type
        /// </summary>
        /// <param name="name">the name of the new route type</param>
        /// <returns>the new route type</returns>
        [HttpPost]
        [Route("types")]
        [UserHasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<RouteTypeViewModel>> PostNewRouteTypeAsync([FromBody] string name)
        {
            var newRouteType = await preferRoutesService.PostNewRouteTypeAsync(name);

            return Ok(newRouteType);
        }

        /// <summary>
        /// Update an existing route type
        /// </summary>
        /// <param name="id">route type id</param>
        /// <param name="name">route type name to be updated</param>
        /// <returns>the updated route type</returns>
        [HttpPatch]
        [Route("types/{id}")]
        [UserHasRole(Role = UserRoles.ADMIN)]
        public async Task<ActionResult<RouteTypeViewModel>> PatchRouteTypeAsync(int id, [FromBody] string name)
        {
            var routeTypeViewModel = await preferRoutesService.PatchRouteTypeAsync(id, name);

            return Ok(routeTypeViewModel);
        }
    }
}
