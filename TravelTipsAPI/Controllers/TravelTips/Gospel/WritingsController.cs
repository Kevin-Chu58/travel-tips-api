using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_gospel;
using TravelTipsAPI.ViewModels.db_search;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.GospelSchema;
using static TravelTipsAPI.Services.TravelTipsServices.RoleSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Gospel
{
    [Route("api/[controller]")]
    public class WritingsController(
        IUsersService usersService,
        IUserRolesService userRolesService,
        IWritingsService writingsService
    ) : TravelTipsControllerBase
    {
        // writings

        /// <summary>
        /// Get a list of writings by params
        /// </summary>
        /// <param name="createdByAuthId">writing writer auth id</param>
        /// <param name="title">writing title</param>
        /// <param name="labelSlug">writing label slug</param>
        /// <param name="isRestricted">whether the future writings are included</param>
        /// <param name="isDesc">whether in descending or ascending order</param>
        /// <returns>a list of writings that fit the params</returns>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<WritingViewModel>>> GetWritingsByParams(
            [FromQuery] string? createdByAuthId = null,
            string? title = null,
            string? labelSlug = null,
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
            WritingLabel? label = null;
            if (labelSlug != null)
                label = writingsService.GetLabelBySlug(labelSlug);

            var writings = await writingsService.GetWritingsByParams(
                createdBy: user?.Id ?? null,
                title: title,
                label: label,
                isRestricted: isRestricted,
                isDesc: isDesc
            );

            return Ok(writings);
        }

        /// <summary>
        /// Get a writing by id
        /// </summary>
        /// <param name="id">writing id</param>
        /// <param name="isRestricted">whether user can see future writings</param>
        /// <returns>the writing with content</returns>
        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<WritingViewModel>> GetWritingById(
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
                var writing = writingsService.GetWritingById(
                    id: id,
                    allowNull: true,
                    isRestricted: isRestricted
                );
                if (writing is null)
                    return NotFound(Messages.WritingNotFound);

                // include actual writing content in the result
                var result = await writingsService.GetWritingViewModel(writing, true);
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
        public ActionResult<int> GetWritingOrderById(int id)
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var writing = writingsService.GetWritingById(id);
            if (writing is null || writing.LabelId is null)
                return NotFound(Messages.WritingNotFound);

            // verify user is writer when it is a future writing
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (writing.PublishAt > today)
            {
                var isWriter = userRolesService.IsWriter(userId);
                if (!isWriter)
                    return BadRequest(Messages.WritingUnauthorized);
            }

            var order = writingsService.GetWritingOrder(writing);
            return Ok(order);
        }

        [HttpGet]
        [Route("{labelSlug}/{order}")]
        [AllowAnonymous]
        public async Task<ActionResult<WritingViewModel>> GetWritingByLabelOrder(
            string labelSlug,
            int order
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // get label from label slug
            var label = writingsService.GetLabelBySlug(labelSlug);
            if (label is null)
                return NotFound(Messages.WritingLabelNotFound);

            if (order <= 0)
                return NotFound(Messages.WritingNotFound);

            var writing = writingsService.GetWritingByLabelOrder(label, order);
            if (writing is null)
                return NotFound(Messages.WritingNotFound);

            // verify user is writer when it is a future writing
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (writing.PublishAt > today)
            {
                var isWriter = userRolesService.IsWriter(userId);
                if (!isWriter)
                    return BadRequest(Messages.WritingUnauthorized);
            }

            var result = await writingsService.GetWritingViewModel(writing, true);
            return Ok(result);
        }

        /// <summary>
        /// Get the latest writings
        /// </summary>
        /// <returns>a list of latest writings</returns>
        //[HttpGet]
        //[Route("latest")]
        //[AllowAnonymous]
        //public ActionResult<IEnumerable<WritingViewModel>> GetLatestWritings()
        //{
        //    var writings = writingsService.GetLatestWritings();
        //    return Ok(writings);
        //}

        /// <summary>
        /// Get a list of my writings
        /// </summary>
        /// <returns>a list of my writings</returns>
        [HttpGet]
        [Route("my")]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<IEnumerable<WritingViewModel>>> GetMyWritings()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var writings = await writingsService.GetWritingsByParams(
                createdBy: userId,
                isRestricted: true
            );
            return Ok(writings);
        }

        /// <summary>
        /// Create a new writing
        /// </summary>
        /// <param name="newWriting">new writing</param>
        /// <returns>the new writing</returns>
        [HttpPost]
        [Route("")]
        [IsOwner(Resource = Resources.NONE)]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<WritingViewModel>> PostNewWriting(
            [FromBody] WritingPostViewModel newWriting
        )
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            if (newWriting.LabelId != null)
            {
                var writingLabel = writingsService.GetLabelById((int)newWriting.LabelId, true);

                if (writingLabel?.Type != "Topic")
                    return BadRequest(Messages.WritingLabelTypeInvalid);
            }

            try
            {
                var writing = await writingsService.PostWriting(newWriting, userId);
                return Ok(writing);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing writing
        /// </summary>
        /// <param name="id">writing id</param>
        /// <param name="writingPatch">writing details to be updataed</param>
        /// <returns>the updated writing</returns>
        [HttpPatch]
        [Route("{id}")]
        [IsOwner(Resource = Resources.WRITINGS)]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<WritingViewModel>> PatchWriting(
            int id,
            [FromBody] WritingPatchViewModel writingPatch
        )
        {
            if (writingPatch.LabelId != null)
            {
                var writingLabel = writingsService.GetLabelById((int)writingPatch.LabelId, true);

                if (writingLabel?.Type != "Topic")
                    return BadRequest(Messages.WritingLabelTypeInvalid);
            }

            var oldWriting = writingsService.GetWritingById(id, true, true);

            var writing = await writingsService.PatchWriting(oldWriting!, writingPatch);
            return Ok(writing);
        }

        /// <summary>
        /// Delete a writing by its id
        /// </summary>
        /// <param name="id">writing id</param>
        /// <returns>the deleted writing id</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.WRITINGS)]
        [HasRole(Role = UserRoles.WRITER)]
        public async Task<ActionResult<int>> DeleteWriting(int id)
        {
            var oldWriting = writingsService.GetWritingById(id, true, true);

            var deletedWritingId = await writingsService.DeleteWriting(oldWriting!);
            return Ok(deletedWritingId);
        }

        // writing labels

        /// <summary>
        /// Get search result of writing labels by params, separated by the topics
        /// </summary>
        /// <param name="name">writing name</param>
        /// <param name="parentLabelId">writing parent label id</param>
        /// <param name="type">writing label type</param>
        /// <param name="timestamp">when was this request sent</param>
        /// <returns>the search result of writing labels that fit the params</returns>
        [HttpGet]
        [Route("labels")]
        [AllowAnonymous]
        public ActionResult<SearchResult<WritingLabelSearchResult>> GetLabelsByParams(
            [FromQuery] string? name = null,
            int? parentLabelId = null,
            string? type = null,
            int? timestamp = null
        )
        {
            var writingLabels = writingsService.GetLabelsByParams(
                name: name,
                parentLabelId: parentLabelId,
                type: type
            );

            var categories = writingLabels.Where(l => l.Type == "Category").ToList();
            var topics = writingLabels.Where(l => l.Type == "Topic").ToList();

            var labelResult = new WritingLabelSearchResult
            {
                Categories = categories,
                Topics = topics,
            };
            var result = new SearchResult<WritingLabelSearchResult>
            {
                Result = labelResult,
                Timestamp = timestamp,
            };

            return Ok(result);
        }

        /// <summary>
        /// Get complete writing label by slug
        /// </summary>
        /// <param name="slug">slug</param>
        /// <returns>the complete writing label</returns>
        [HttpGet]
        [Route("labels/{slug}")]
        [AllowAnonymous]
        public ActionResult<WritingLabelCompleteViewModel> GetCompleteWritingLabelBySlug(
            string slug
        )
        {
            var label = writingsService.GetLabelBySlug(slug);
            if (label is null)
                return NotFound(Messages.WritingLabelNotFound);

            var completeLabel = writingsService.BuildWritingLabelComplete(label.Id);

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
        public async Task<ActionResult<WritingLabelViewModel>> PostNewLabel(
            [FromQuery] string name,
            string type,
            int? parentLabelId = null
        )
        {
            try
            {
                if (type == "Category" && parentLabelId != null)
                {
                    return BadRequest(Messages.WritingLabelTypeInvalid);
                }

                if (type == "Topic" && parentLabelId is null)
                {
                    return BadRequest(Messages.WritingLabelTypeInvalid);
                }

                var label = await writingsService.PostNewLabel(name, type, parentLabelId);
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
        public async Task<ActionResult<WritingLabelViewModel>> UpdateLabel(
            int id,
            [FromQuery] string name,
            int? parentLabelId = null
        )
        {
            try
            {
                var oldLabel = writingsService.GetLabelById(id, true);
                if (oldLabel is null)
                    return NotFound(Messages.WritingLabelNotFound);

                if (oldLabel.Type == "Category" && parentLabelId != null)
                {
                    return BadRequest(Messages.WritingLabelTypeInvalid);
                }

                if (oldLabel.Type == "Topic" && parentLabelId is null)
                {
                    return BadRequest(Messages.WritingLabelTypeInvalid);
                }

                var label = await writingsService.UpdateLabel(oldLabel, name);
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
                var oldLabel = writingsService.GetLabelById(id);

                var deletedLabelId = await writingsService.DeleteLabel(oldLabel!);
                return Ok(deletedLabelId);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
