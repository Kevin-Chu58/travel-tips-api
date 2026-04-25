using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.SearchSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Search
{
    [Route("api/[controller]")]
    public class RegionsController(IRegionsService regionsService) : TravelTipsControllerBase
    {
        [HttpGet]
        [Route("browse")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<RegionViewModel>> GetRegionNamesByParams(
            [FromQuery] string type,
            string? name = null,
            int? parentRegionId = null
        )
        {
            var regions = regionsService.GetRegionsByParams(type, name, parentRegionId);
            return Ok(regions);
        }

        [HttpGet]
        [Route("{id}")]
        [IsOwner(Resource = Resources.NONE)]
        public ActionResult<RegionCompleteViewModel> GetRegionCompleteById(int id)
        {
            var regionComplete = regionsService.BuildRegionComplete(id);
            if (regionComplete == null)
                return NotFound(Messages.RegionNotFound);

            return Ok(regionComplete);
        }
    }
}
