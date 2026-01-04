using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Models.TravelTipsModels;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Search
{
    [Route("api/[controller]")]
    public class RegionsController(IRegionsService regionsService) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("browse")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<Region>> GetRegionNamesByParams(
            [FromQuery] string type,
            string? name = null,
            int? parentRegionId = null
        )
        {
            var regions = regionsService.GetRegionsByParams(type, name, parentRegionId);
            return Ok(regions);
        }
    }
}
