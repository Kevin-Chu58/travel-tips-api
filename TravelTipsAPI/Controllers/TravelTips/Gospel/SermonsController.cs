using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_gospel;
using TravelTipsAPI.ViewModels.db_search;
using TravelTipsAPI.ViewModels.db_sermon;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.GospelSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Gospel
{
    [Route("api/[controller]")]
    public class SermonsController(
        IUsersService usersService,
        IUserRolesService userRolesService,
        ISermonsService sermonsService
    ) : TravelTipsControllerBase
    {
        // sermons

        /// <summary>
        /// Get a list of sermons by params
        /// </summary>
        /// <param name="createdByAuthId">sermon writer auth id</param>
        /// <param name="title">sermon title</param>
        /// <param name="labelSlug">sermon label slug</param>
        /// <param name="isBanner">whether the sermon is bannered</param>
        /// <param name="isRestricted">whether the future sermons are included</param>
        /// <param name="isDesc">whether in descending or ascending order</param>
        /// <returns>a list of sermons that fit the params</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        [SetUserId]
        public ActionResult<IEnumerable<SermonViewModel>> GetSermonsByParams(
            string? createdByAuthId = null,
            string? title = null,
            string? labelSlug = null,
            bool? isBanner = null,
            bool isRestricted = false,
            bool isDesc = true
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // re-affirm access to restricted information
            var isWriter = userRolesService.IsWriter(userId);
            if (!isWriter)
                isRestricted = false;

            // get user id from authId
            User? user = null;
            if (createdByAuthId != null)
                user = usersService.GetUserByUserId(createdByAuthId);

            // get label from label slug
            SermonLabel? label = null;
            if (labelSlug != null)
                label = sermonsService.GetLabelBySlug(labelSlug);

            var sermons = sermonsService.GetSermonsByParams(
                createdBy: user?.Id ?? null,
                title: title,
                label: label,
                isBanner: isBanner,
                isRestricted: isRestricted,
                isDesc: isDesc
            );

            return Ok(sermons);
        }

        /// <summary>
        /// Get a sermon by id
        /// </summary>
        /// <param name="id">sermon id</param>
        /// <param name="isRestricted">whether user can see future sermons</param>
        /// <returns>the sermon with content</returns>
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        [SetUserId]
        public async Task<ActionResult<SermonViewModel>> GetSermonById(
            int id,
            [FromQuery] bool isRestricted = false
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // re-affirm access to restricted information
            var isWriter = userRolesService.IsWriter(userId);
            if (!isWriter)
                isRestricted = false;

            try
            {
                var sermon = sermonsService.GetSermonById(
                    id: id,
                    allowNull: true,
                    isRestricted: isRestricted
                );
                if (sermon is null)
                    return NotFound(Messages.SermonNotFound);

                // include actual sermon content in the result
                var result = await sermonsService.GetSermonViewModel(sermon, true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("order/{id}")]
        [AllowAnonymous]
        [SetUserId]
        public ActionResult<int> GetSermonOrderById(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var sermon = sermonsService.GetSermonById(id);
            if (sermon is null || sermon.LabelId is null)
                return NotFound(Messages.SermonNotFound);

            // verify user is writer when it is a future sermon
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (sermon.PublishAt > today)
            {
                var isWriter = userRolesService.IsWriter(userId);
                if (!isWriter)
                    return BadRequest(Messages.SermonUnauthorized);
            }

            var order = sermonsService.GetSermonOrder(sermon);
            return Ok(order);
        }

        [HttpGet]
        [Route("{labelSlug}/{order}")]
        [AllowAnonymous]
        [SetUserId]
        public async Task<ActionResult<SermonViewModel>> GetSermonByLabelOrder(
            string labelSlug,
            int order
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // get label from label slug
            var label = sermonsService.GetLabelBySlug(labelSlug);
            if (label is null)
                return NotFound(Messages.SermonLabelNotFound);

            if (order <= 0)
                return NotFound(Messages.SermonNotFound);

            var sermon = sermonsService.GetSermonByLabelOrder(label, order);
            if (sermon is null)
                return NotFound(Messages.SermonNotFound);

            // verify user is writer when it is a future sermon
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (sermon.PublishAt > today)
            {
                var isWriter = userRolesService.IsWriter(userId);
                if (!isWriter)
                    return BadRequest(Messages.SermonUnauthorized);
            }

            var result = await sermonsService.GetSermonViewModel(sermon, true);
            return Ok(result);
        }

        /// <summary>
        /// Get the latest sermons
        /// </summary>
        /// <returns>a list of latest sermons</returns>
        [HttpGet]
        [Route("latest")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<SermonViewModel>> GetLatestSermons()
        {
            var sermons = sermonsService.GetLatestSermons();
            return Ok(sermons);
        }

        /// <summary>
        /// Get a list of my sermons
        /// </summary>
        /// <returns>a list of my sermons</returns>
        [HttpGet]
        [Route("my")]
        [HasRole(Role = UserRoles.WRITER)]
        public ActionResult<IEnumerable<SermonViewModel>> GetMySermons()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var sermons = sermonsService.GetSermonsByParams(createdBy: userId, isRestricted: true);
            return Ok(sermons);
        }

        /// <summary>
        /// Create a new sermon
        /// </summary>
        /// <param name="newSermon">new sermon</param>
        /// <returns>the new sermon</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<SermonViewModel>> PostNewSermon(
            [FromBody] SermonPostViewModel newSermon
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            if (newSermon.LabelId != null)
            {
                var sermonLabel = sermonsService.GetLabelById((int)newSermon.LabelId, true);

                if (sermonLabel?.Type != "Topic")
                    return BadRequest(Messages.SermonLabelTypeInvalid);
            }

            try
            {
                var sermon = await sermonsService.PostSermon(newSermon, userId);
                return Ok(sermon);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing sermon
        /// </summary>
        /// <param name="id">sermon id</param>
        /// <param name="sermonPatch">sermon details to be updataed</param>
        /// <returns>the updated sermon</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.SERMONS)]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<SermonViewModel>> PatchSermon(
            int id,
            [FromBody] SermonPatchViewModel sermonPatch
        )
        {
            if (sermonPatch.LabelId != null)
            {
                var sermonLabel = sermonsService.GetLabelById((int)sermonPatch.LabelId, true);

                if (sermonLabel?.Type != "Topic")
                    return BadRequest(Messages.SermonLabelTypeInvalid);
            }

            var oldSermon = sermonsService.GetSermonById(id, true, true);

            var sermon = await sermonsService.PatchSermon(oldSermon!, sermonPatch);
            return Ok(sermon);
        }

        /// <summary>
        /// Delete a sermon by its id
        /// </summary>
        /// <param name="id">sermon id</param>
        /// <returns>the deleted sermon id</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.SERMONS)]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<int>> DeleteSermon(int id)
        {
            var oldSermon = sermonsService.GetSermonById(id, true, true);

            var deletedSermonId = await sermonsService.DeleteSermon(oldSermon!);
            return Ok(deletedSermonId);
        }

        // sermon labels

        /// <summary>
        /// Get search result of sermon labels by params, separated by the topics
        /// </summary>
        /// <param name="name">sermon name</param>
        /// <param name="parentLabelId">sermon parent label id</param>
        /// <param name="type">sermon label type</param>
        /// <param name="timestamp">when was this request sent</param>
        /// <returns>the search result of sermon labels that fit the params</returns>
        [HttpGet]
        [Route("labels")]
        [AllowAnonymous]
        public ActionResult<SearchResult<SermonLabelSearchResult>> GetLabelsByParams(
            [FromQuery] string? name = null,
            int? parentLabelId = null,
            string? type = null,
            int? timestamp = null
        )
        {
            var sermonLabels = sermonsService.GetLabelsByParams(
                name: name,
                parentLabelId: parentLabelId,
                type: type
            );

            var categories = sermonLabels.Where(l => l.Type == "Category").ToList();
            var topics = sermonLabels.Where(l => l.Type == "Topic").ToList();

            var labelResult = new SermonLabelSearchResult
            {
                Categories = categories,
                Topics = topics,
            };
            var result = new SearchResult<SermonLabelSearchResult>
            {
                Result = labelResult,
                Timestamp = timestamp,
            };

            return Ok(result);
        }

        /// <summary>
        /// Get complete sermon label by slug
        /// </summary>
        /// <param name="slug">slug</param>
        /// <returns>the complete sermon label</returns>
        [HttpGet]
        [Route("labels/{slug}")]
        [AllowAnonymous]
        public ActionResult<SermonLabelCompleteViewModel> GetCompleteSermonLabelBySlug(string slug)
        {
            var label = sermonsService.GetLabelBySlug(slug);
            if (label is null)
                return NotFound(Messages.SermonLabelNotFound);

            var completeLabel = sermonsService.BuildSermonLabelComplete(label.Id);

            return Ok(completeLabel);
        }

        /// <summary>
        /// Create a new label
        /// </summary>
        /// <param name="name">label name</param>
        /// <param name="type">label type</param>
        /// <param name="parentLabelId">parent label id</param>
        /// <returns>the new label</returns>
        [HttpPost]
        [Route("labels")]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<SermonLabelViewModel>> PostNewLabel(
            [FromQuery] string name,
            string type,
            int? parentLabelId = null
        )
        {
            try
            {
                if (type == "Category" && parentLabelId != null)
                {
                    return BadRequest(Messages.SermonLabelTypeInvalid);
                }

                if (type == "Topic" && parentLabelId is null)
                {
                    return BadRequest(Messages.SermonLabelTypeInvalid);
                }

                var label = await sermonsService.PostNewLabel(name, type, parentLabelId);
                return Ok(label);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing label
        /// </summary>
        /// <param name="id">label id</param>
        /// <param name="name">label new name</param>
        /// <param name="parentLabelId">parent label id</param>
        /// <returns>the updated label</returns>
        [HttpPatch]
        [Route("labels/{id}")]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<SermonLabelViewModel>> UpdateLabel(
            int id,
            [FromQuery] string name,
            int? parentLabelId = null
        )
        {
            try
            {
                var oldLabel = sermonsService.GetLabelById(id, true);
                if (oldLabel is null)
                    return NotFound(Messages.SermonLabelNotFound);

                if (oldLabel.Type == "Category" && parentLabelId != null)
                {
                    return BadRequest(Messages.SermonLabelTypeInvalid);
                }

                if (oldLabel.Type == "Topic" && parentLabelId is null)
                {
                    return BadRequest(Messages.SermonLabelTypeInvalid);
                }

                var label = await sermonsService.UpdateLabel(oldLabel, name);
                return Ok(label);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a label by id
        /// </summary>
        /// <param name="id">label id</param>
        /// <returns>the deleted label id</returns>
        [HttpDelete]
        [Route("labels/{id}")]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<int>> DeleteLabel(int id)
        {
            try
            {
                var oldLabel = sermonsService.GetLabelById(id);

                var deletedLabelId = await sermonsService.DeleteLabel(oldLabel!);
                return Ok(deletedLabelId);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
