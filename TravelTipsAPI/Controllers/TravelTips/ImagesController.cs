using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Authorization;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.db_image;
using TravelTipsAPI.ViewModels.db_search;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Constants.Enums.ImageEnum;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;
using static TravelTipsAPI.Services.TravelTipsServices.FeedSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;
using static TravelTipsAPI.Utils.ObjectUtils;
using static TravelTipsAPI.ViewModels.db_search.SearchCursors;

namespace TravelTipsAPI.Controllers.TravelTips
{
    [Route("api/[controller]")]
    public class ImagesController(
        TravelTipsContext context,
        IImagesService imagesService,
        IUsersService usersService,
        IBusinessesService businessesService
    ) : TravelTipsControllerBase
    {
        // images

        /// <summary>
        /// Get image by user id
        /// </summary>
        /// <returns>the images the user owns</returns>
        [HttpGet]
        [Route("my")]
        [IsOwner(Resource = Resources.NONE)]
        public async Task<ActionResult<SearchResults<ImageViewModel>>> GetImagesByUserId(
            string? cursor
        )
        {
            var limit = Global.IMAGE_DEFAULT_LIMIT;
            var userId = (int)(HttpContext.Items["user_id"] ?? 0);

            // decode cursor if provided
            GeneralCursor? generalCursor = null;
            if (!string.IsNullOrEmpty(cursor))
            {
                generalCursor = DecodeCursor<GeneralCursor>(cursor);
                if (generalCursor is null)
                    return BadRequest(Messages.CursorInvalid);
            }

            var imageIds = imagesService
                .GetImageIdsByUserId(userId, limit, generalCursor)
                .ToArray();

            var images = await imagesService.GetImagesByIds(imageIds);

            // encode cursor
            var imageList = images.ToList();
            string? newCursor = null;
            if (imageList.Count == limit)
            {
                var lastImage = imageList.Last();
                newCursor = EncodeCursor(new GeneralCursor { Id = lastImage.Id });
            }

            var result = new SearchResults<ImageViewModel>
            {
                Results = imageList,
                Cursor = newCursor,
            };

            return Ok(result);
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
            var image = imagesService.FindImageById(id);
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
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var imageViewModel = await imagesService.PostNewImageAsync(
                    file,
                    userId,
                    name,
                    type: null
                );

                return Ok(imageViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update image name
        /// </summary>
        /// <param name="id">image id</param>
        /// <param name="name">new image name</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/name/{name}")]
        [IsOwner(Resource = Resources.IMAGES)]
        public async Task<ActionResult> UpdateImageName(int id, string name)
        {
            try
            {
                var image = imagesService.FindImageById(id);
                if (image == null)
                    return NotFound(Messages.ImageNotFound);

                if (image.Type == "banner")
                    return BadRequest(Messages.ImageUnauthorized);

                await imagesService.UpdateImageName(image, name);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete an existing image
        /// </summary>
        /// <param name="id">image id</param>
        /// <returns>the deleted image id</returns>
        [HttpDelete]
        [Route("{id}")]
        [IsOwner(Resource = Resources.IMAGES)]
        public async Task<ActionResult<int>> DeleteImage(int id)
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var image = imagesService.FindImageById(id);
                if (image == null)
                    return NotFound(Messages.ImageNotFound);

                if (image.Type == "banner")
                    return BadRequest(Messages.ImageUnauthorized);

                var user = usersService.GetUserById(userId);
                if (user.ImageId == id)
                    return BadRequest(Messages.ImageUserPicture);

                await imagesService.DeleteImageAsync(image);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // banner images

        /// <summary>
        /// Get banner images
        /// </summary>
        /// <returns>a list of banner images</returns>
        [HttpGet]
        [Route("banner")]
        [HasRole(Role = UserRoles.BANNER_MAN)]
        public async Task<ActionResult<IEnumerable<ImageViewModel>>> GetBannerImages()
        {
            var imageIds = imagesService.GetBannerImageIds().ToArray();

            var images = await imagesService.GetImagesByIds(imageIds);

            return Ok(images);
        }

        /// <summary>
        /// Upload banner image
        /// </summary>
        /// <param name="name">image name</param>
        /// <param name="file">image data</param>
        /// <returns>new banner image</returns>
        [HttpPost]
        [Route("banner")]
        [HasRole(Role = UserRoles.BANNER_MAN)]
        public async Task<ActionResult<ImageViewModel>> UploadBannerImage(
            [FromForm] string? name,
            IFormFile file
        )
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var imageViewModel = await imagesService.PostNewImageAsync(
                    file,
                    userId,
                    name,
                    type: ImageType.Banner
                );

                return Ok(imageViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update banner image name
        /// </summary>
        /// <param name="id">image id</param>
        /// <param name="name">uew image name</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("banner/{id}/name/{name}")]
        [HasRole(Role = UserRoles.BANNER_MAN)]
        public async Task<ActionResult> UpdateBannerImageName(int id, string name)
        {
            try
            {
                var image = imagesService.FindImageById(id);
                if (image == null)
                    return NotFound(Messages.ImageNotFound);

                if (image.Type != "banner")
                    return BadRequest(Messages.ImageUnauthorized);

                await imagesService.UpdateImageName(image, name);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a banner image
        /// </summary>
        /// <param name="id">image id</param>
        /// <returns>the deleted image id</returns>
        [HttpDelete]
        [Route("banner/{id}")]
        [HasRole(Role = UserRoles.BANNER_MAN)]
        public async Task<ActionResult<int>> DeleteBannerImage(int id)
        {
            try
            {
                var image = imagesService.FindImageAndBannerCountById(out int bannerCount, id);
                if (image == null)
                    return NotFound(Messages.ImageNotFound);

                if (image.Type != "banner")
                    return BadRequest(Messages.ImageUnauthorized);

                if (bannerCount > 0)
                    return BadRequest(Messages.ImageBannerAttached);

                await imagesService.DeleteImageAsync(image);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // business images

        /// <summary>
        /// Upload business image, overwrite image data if already exist
        /// </summary>
        /// <param name="file">image data</param>
        /// <returns>new business image</returns>
        [HttpPost]
        [Route("business/{id}")]
        [IsOwner(Resource = Resources.BUSINESSES)]
        public async Task<ActionResult<ImageViewModel>> UploadBusinessImage(int id, IFormFile file)
        {
            try
            {
                var userId = (int)(HttpContext.Items["user_id"] ?? 0);

                var tx = context.Database.BeginTransaction();

                var business = businessesService.FindBusinessById(id);
                if (business == null)
                    return NotFound(Messages.BusinessNotFound);

                ImageViewModel imageViewModel;

                if (business.ImageId == null)
                {
                    imageViewModel = await imagesService.PostNewImageAsync(
                        file,
                        userId,
                        name: null,
                        type: ImageType.Business
                    );
                }
                else
                {
                    var image = imagesService.FindImageById((int)business.ImageId);
                    if (image == null)
                        return NotFound(Messages.ImageNotFound);

                    imageViewModel = await imagesService.OverwriteImageAsync(image, file);
                }

                business.ImageId = imageViewModel.Id;
                await context.SaveChangesAsync();

                await tx.CommitAsync();

                return Ok(imageViewModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a business image
        /// </summary>
        /// <param name="id">image id</param>
        /// <returns>the deleted image id</returns>
        [HttpDelete]
        [Route("business/{id}")]
        [IsOwner(Resource = Resources.BUSINESSES)]
        public async Task<ActionResult> DeleteBusinessImage(int id)
        {
            try
            {
                var business = businessesService.FindBusinessById(id);
                if (business == null)
                    return NotFound(Messages.BusinessNotFound);

                var image = imagesService.FindImageById((int)business.ImageId);
                if (image == null)
                    return NotFound(Messages.ImageNotFound);

                if (image.Type != GetImageTypeStr(ImageType.Business))
                    return BadRequest(Messages.ImageUnauthorized);

                var tx = await context.Database.BeginTransactionAsync();

                await businessesService.RemoveBusinessImage(business);

                await imagesService.DeleteImageAsync(image);

                await tx.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
