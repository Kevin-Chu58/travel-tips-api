using System.Net.Mail;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Services.TravelTipsServices;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Controllers.TravelTips
{
    [Route("api/[controller]")]
    public class ImagesController(IImagesService imagesService) : TravelTipsControllerBase
    {
        /// <summary>
        /// Get image by user id
        /// </summary>
        /// <returns>the images the user owns</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<ImageViewModel>> GetImagesByUserId()
        {
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            var imageIds = imagesService.GetImageIdsByUserId(userId).ToArray();

            var images = await imagesService.GetImagesByIds(imageIds);

            return Ok(images);
        }

        /// <summary>
        /// Get the image file by image id
        /// </summary>
        /// <param name="id">image id</param>
        /// <returns>the file with the image id</returns>
        [HttpGet]
        [Route("download/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetImageFile(int id)
        {
            var image = imagesService.GetImageById(id);
            if (image == null)
                return NotFound();

            var bytes = await imagesService.DownloadImageAsync(image.CreatedBy, image.Guid);

            var base64 = Convert.ToBase64String(bytes);
            return Ok(new { base64 });
        }

        /// <summary>
        /// upload an image to firebase
        /// </summary>
        /// <param name="name">name of the image</param>
        /// <param name="file">file of the image</param>
        /// <returns>image view model created</returns>
        [HttpPost]
        [Route("")]
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

        [HttpPatch]
        [Route("{id}/name/{name}")]
        [IsOwner(Resource = Resources.IMAGES)]
        public async Task<ActionResult> UpdateImageName(int id, string name)
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var image = imagesService.GetImageById(id);
                if (image == null)
                    return NotFound(Messages.ImageNotFound);

                await imagesService.UpdateImageName(image, name);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.IMAGES)]
        public async Task<ActionResult<int>> DeleteImage(int id)
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var image = imagesService.GetImageById(id);
                if (image == null)
                    return NotFound(Messages.ImageNotFound);

                await imagesService.DeleteImageAsync(image);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
