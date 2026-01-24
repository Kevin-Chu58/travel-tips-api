using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_gospel;
using TravelTipsAPI.ViewModels.db_sermon;

namespace TravelTipsAPI.Controllers.TravelTips.Gospel
{
    [ApiController]
    public class SermonsController : TravelTipsControllerBase
    {
        // sermons

        [HttpGet]
        [Route("latest")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<SermonViewModel>> GetLatestSermons()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        [SetUserId]
        public ActionResult<IEnumerable<SermonViewModel>> GetSermonsByParams(
            string? createdByAuthId = null,
            string? title = null,
            int? labelId = null,
            bool? isBanner = null,
            bool isRestricted = true,
            bool isDesc = false
        )
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        [SetUserId]
        public ActionResult<SermonViewModel> GetSermonById(
            int id,
            [FromQuery] bool isRestricted = true
        )
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        [HasRole(Role = UserRoles.WRITER)]
        public Task<ActionResult<SermonViewModel>> PostNewSermon(
            [FromBody] SermonPostViewModel newSermon
        )
        {
            throw new NotImplementedException();
        }

        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.SERMONS)]
        [HasRole(Role = UserRoles.WRITER)]
        public Task<ActionResult<SermonViewModel>> PatchSermon(
            int id,
            [FromBody] SermonPatchViewModel sermonPatch
        )
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.SERMONS)]
        [HasRole(Role = UserRoles.WRITER)]
        public Task<ActionResult<int>> DeleteSermon(int id)
        {
            throw new NotImplementedException();
        }

        // sermon labels
    }
}
