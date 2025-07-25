using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.ViewModels.db_image;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Controllers.TravelTips
{
    [Route("api/[controller]")]
    public class ImagesController(IImagesService imagesService) : TravelTipsControllerBase
    {
        [HttpPost]
        [Route("upload")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<ImageViewModel>> UploadImage(
            [FromForm] string? name,
            IFormFile file
        )
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                using var stream = file.OpenReadStream();
                var contentType = file.ContentType;

                var imageViewModel = await imagesService.PostNewImageAsync(
                    stream,
                    contentType,
                    userId,
                    name
                );

                return Ok(imageViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
