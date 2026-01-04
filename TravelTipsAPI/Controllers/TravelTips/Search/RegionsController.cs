using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Search
{
    [Route("api/[controller]")]
    public class RegionsController(IRegionsService regionsService) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("browse")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<string>> GetRegionNamesByParams(
            [FromQuery] string type,
            string? name = null,
            int? parentRegionId = null
        )
        {
            var regionNames = regionsService.GetRegionsByParams(type, name, parentRegionId);
            return Ok(regionNames);
        }
    }
}
