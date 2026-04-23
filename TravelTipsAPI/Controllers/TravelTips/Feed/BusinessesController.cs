using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_feed;
using static TravelTipsAPI.Constants.Enums.AdEnum;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Controllers.TravelTips.Feed
{
    [Route("api/[controller]")]
    public class BusinessesController(
        IBusinessesService businessesService,
        IImagesService imagesService
    ) : TravelTipsControllerBase
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
            //var result = await AppendImagesToBusinessesAsync(businesses);

            return Ok(businesses);
        }

        /// <summary>
        /// Get my non-pending businesses
        /// </summary>
        /// <returns>my non-pending businesses</returns>
        //[HttpGet]
        //[Route("my/non-pending")]
        //[IsOwner(Resource = Resources.NONE)]
        //public ActionResult<IEnumerable<BusinessViewModel>> GetMyNonPendingBusinesses()
        //{
        //    var userId = (int)(HttpContext.Items["user_id"] ?? 0);

        //    var businesses = businessesService.GetBusinessesByParams(
        //        userId,
        //        null,
        //        AdStatus.Pending
        //    );
        //    return Ok(businesses);
        //}

        /// <summary>
        /// Get a business by id
        /// </summary>
        /// <param name="id">business id</param>
        /// <returns>the business with the id</returns>
        [HttpGet]
        [Route("{id}")]
        [IsOwner(Resource = Resources.BUSINESSES)]
        public async Task<ActionResult<BusinessViewModel>> GetBusinessById(int id)
        {
            var business = businessesService.FindBusinessById(id);
            if (business == null)
                return NotFound(Messages.BusinessNotFound);

            var result = await AppendImagesToBusinessesAsync([(BusinessViewModel)business]);

            return Ok(result.First());
        }

        /// <summary>
        /// Get businesses by params
        /// </summary>
        /// <returns>a list of businesses fits the params</returns>
        [HttpGet]
        [Route("")]
        [HasRole(Role = UserRoles.REVIEWER)]
        public async Task<ActionResult<IEnumerable<BusinessViewModel>>> GetBusinessesByParams(
            [FromQuery] int? userId,
            [FromQuery] int? status
        )
        {
            AdStatus? statusEnum = status != null ? (AdStatus)status : null;

            var businesses = businessesService.GetBusinessesByParams(userId, statusEnum);
            var result = await AppendImagesToBusinessesAsync(businesses);

            return Ok(result);
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
        /// <returns>the new status</returns>
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
        /// <returns>the new status</returns>
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

        private async Task<IEnumerable<BusinessViewModel>> AppendImagesToBusinessesAsync(
            IEnumerable<BusinessViewModel> businesses
        )
        {
            var businessList = businesses.ToList();
            var imageIds = businesses
                .Where(bs => bs.ImageId != null)
                .Select(bs => (int)bs.ImageId!)
                .ToArray();

            var images = await imagesService.GetImagesByIds(imageIds);

            // Create a lookup dictionary for O(1) retrieval
            var imageDict = images.ToDictionary(img => img.Id, img => img);

            // Map each business to its corresponding image
            foreach (var business in businessList)
            {
                if (
                    business.ImageId != null
                    && imageDict.TryGetValue((int)business.ImageId, out var image)
                )
                {
                    business.Picture = image.Url;
                }
            }

            return businessList;
        }
    }
}
