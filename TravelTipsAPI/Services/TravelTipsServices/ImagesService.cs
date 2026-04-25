using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Firebase;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_image;
using static TravelTipsAPI.Constants.Enums.ImageEnum;
using static TravelTipsAPI.Services.AzureKeyVaultServices.AzureKeyVaultSchema;
using static TravelTipsAPI.Services.TravelTipsServices.ImageSchema;

namespace TravelTipsAPI.Services.TravelTipsServices
{
    public class ImagesService(
        TravelTipsContext context,
        IConfiguration config,
        IKeyVaultService keyVaultService,
        FirebaseStorageUploader uploader,
        UpstashHttpClient cache
    ) : IImagesService
    {
        private UrlSigner? _urlSigner;

        public Image? FindImageById(int id)
        {
            var image = context.Images.Find(id);
            return image;
        }

        public Image? FindImageAndBannerCountById(out int bannerCount, int id)
        {
            var result = context
                .Images.AsNoTracking()
                .Include(i => i.Banners)
                .Where(i => i.Id == id)
                .Select(i => new { Image = i, i.Banners.Count })
                .FirstOrDefault();

            bannerCount = result?.Count ?? 0;

            return result?.Image;
        }

        public Image? FindImageAndBusinessCountById(out int businessCount, int id)
        {
            var result = context
                .Images.AsNoTracking()
                .Include(i => i.Businesses)
                .Where(i => i.Id == id)
                .Select(i => new { Image = i, i.Businesses.Count })
                .FirstOrDefault();

            businessCount = result?.Count ?? 0;

            return result?.Image;
        }

        /// <summary>
        /// Get a list of images by their ids
        /// </summary>
        /// <param name="ids">ids</param>
        /// <returns>a list of the images</returns>
        public async Task<IEnumerable<ImageViewModel>> GetImagesByIds(int[] ids)
        {
            // keys of the image json
            var keys = ids.Select(id => GetImageUpstashKey(id)).ToArray();

            // check cache first, if does not exist, send request to HereMap API
            var cachesJson = await cache.GetMultipleAsync(keys);

            List<ImageViewModel> imageViewModels = [];

            for (int i = 0; i < cachesJson.Count; i++)
            {
                var cacheJson = cachesJson[i];

                if (cacheJson != null)
                {
                    // if the cache is not expired, convert it to the correct type
                    imageViewModels.Add(JsonSerializer.Deserialize<ImageViewModel>(cacheJson)!);
                }
                else
                {
                    var imageViewModel = await GenerateNewImageAsync(ids[i], key: keys[i]);

                    imageViewModels.Add(imageViewModel);
                }
            }

            return imageViewModels;
        }

        /// <summary>
        /// Generate new image view model from image id and key
        /// </summary>
        /// <param name="id">image id</param>
        /// <param name="key">key to get from upstash</param>
        /// <returns>a newly generated image view model</returns>
        private async Task<ImageViewModel> GenerateNewImageAsync(
            int id = 0,
            Image? image = null,
            string key = ""
        )
        {
            if (image == null)
                image = context.Images.Find(id);

            if (key == "")
                key = $"image:{id}:v{CacheVersion.Image_Version}";

            if (image is null)
            {
                throw new Exception(Messages.ImageNotFound);
            }

            var imageViewModel = (ImageViewModel)image;

            imageViewModel.Url = await GenerateSignedUrlAsync(
                config["Firebase:BucketName"]!,
                $"{imageViewModel.CreatedBy}/{image.Guid}.jpeg",
                TimeSpan.FromDays(7) // Firebase allows maximum 7 days expiration
            );

            // cache it to UpStash
            string jsonString = JsonSerializer.Serialize(imageViewModel);
            await cache.SetWithExpiryAsync(key, jsonString, Time.WEEK_1);

            return imageViewModel;
        }

        /// <summary>
        /// Get a list of image ids by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of image ids</returns>
        public IEnumerable<int> GetImageIdsByUserId(int id)
        {
            var imageIds = context
                .Images.Where(i => i.CreatedBy == id && i.Type == null)
                .Select(i => i.Id)
                .ToList();

            return imageIds;
        }

        public IEnumerable<int> GetBannerImageIds()
        {
            var imageIds = context.Images.Where(i => i.Type == "banner").Select(i => i.Id).ToList();
            return imageIds;
        }

        /// <summary>
        /// Get a list of image ids by trip id
        /// </summary>
        /// <param name="id">trip id</param>
        /// <returns>a list of image ids</returns>
        public IEnumerable<int> GetImageIdsByTripId(int id)
        {
            var imageIds = context
                .TripImages.Where(ti => ti.TripId == id)
                .Select(ti => ti.ImageId)
                .ToList();

            return imageIds;
        }

        /// <summary>
        /// Post new image by uploading to Firebase storage and storing the file path
        /// </summary>
        /// <param name="file">image blob file</param>
        /// <param name="userId">user id</param>
        /// <param name="name">image file name</param>
        /// <param name="type">image type</param>
        /// <returns>the posted image</returns>
        public async Task<ImageViewModel> PostNewImageAsync(
            IFormFile file,
            int userId,
            string? name,
            ImageType? type
        )
        {
            if (file == null || file.Length == 0)
                throw new Exception(Messages.ImageNoFileUpload);

            try
            {
                using var stream = file.OpenReadStream();
                var contentType = file.ContentType;

                if (stream == null || stream.Length == 0)
                    throw new ArgumentException(Messages.ImageStreamEmpty);

                Guid guid = Guid.NewGuid();
                string fileName = $"{guid}{GetExtensionFromContentType(contentType)}";

                // upload image to firebase, throws an exception if failed
                await uploader.UploadFileAsync(
                    stream,
                    contentType,
                    config["Firebase:BucketName"]!,
                    $"{userId}/{fileName}"
                );

                Image newImage = new()
                {
                    Guid = guid,
                    Name = name,
                    CreatedBy = userId,
                    Type = GetImageTypeStr(type),
                };

                context.Images.Add(newImage);
                await context.SaveChangesAsync();

                var newImageViewModel = await GenerateNewImageAsync(image: newImage);

                return newImageViewModel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ImageViewModel> OverwriteImageAsync(Image image, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception(Messages.ImageNoFileUpload);

            try
            {
                using var newStream = file.OpenReadStream();
                var contentType = file.ContentType;

                if (newStream == null || newStream.Length == 0)
                    throw new ArgumentException(Messages.ImageStreamEmpty);

                // Reconstruct the exact filename and path used in the original upload
                string fileName = $"{image.Guid}{GetExtensionFromContentType(contentType)}";
                string storagePath = $"{image.CreatedBy}/{fileName}";

                // Uploading to the same path will overwrite the existing file in Firebase
                await uploader.UploadFileAsync(
                    newStream,
                    contentType,
                    config["Firebase:BucketName"]!,
                    storagePath
                );

                // Invalidate the cache for this image
                var cacheKey = GetImageUpstashKey(image.Id);
                await cache.DeleteKeyAsync(cacheKey);

                var newImageViewModel = await GenerateNewImageAsync(image: image);

                return newImageViewModel;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to overwrite image: {ex.Message}");
            }
        }

        public async Task<byte[]> DownloadImageAsync(int userId, Guid guid)
        {
            var bucket = config["Firebase:BucketName"]!;
            var objectName = $"{userId}/{guid}.jpeg";

            // Get Google credential JSON from Key Vault (you already have this in your service)
            var credentialJson = await keyVaultService.GetJsonSecretAsync(
                config["AzureKeyVault:FirebaseKey"]!
            );
            var credential = GoogleCredential.FromJson(credentialJson);

            using var storage = await StorageClient.CreateAsync(credential);
            using var memoryStream = new MemoryStream();

            await storage.DownloadObjectAsync(bucket, objectName, memoryStream);

            return memoryStream.ToArray();
        }

        public async Task UpdateImageName(Image image, string newName)
        {
            if (newName.Length > 50)
                throw new Exception(Messages.ImageNameTooLong);

            image.Name = newName;
            await context.SaveChangesAsync();

            await GenerateNewImageAsync(image.Id, image);

            return;
        }

        public async Task<ImageRelationViewModel> AttachImageToTrip(int imageId, int tripId)
        {
            // make sure image is not attached on trip
            var tripImage = context.TripImages.FirstOrDefault(ti =>
                ti.ImageId == imageId && ti.TripId == tripId
            );

            if (tripImage != null)
                throw new Exception(Messages.ImageTripAttached);

            TripImage newTripImage = new() { TripId = tripId, ImageId = imageId };

            context.TripImages.Add(newTripImage);
            await context.SaveChangesAsync();

            return (ImageRelationViewModel)newTripImage;
        }

        public async Task<ImageRelationViewModel> DetachImageFromTrip(int imageId, int tripId)
        {
            // make sure image is attached to trip
            var tripImage = context.TripImages.FirstOrDefault(ti =>
                ti.ImageId == imageId && ti.TripId == tripId
            );

            if (tripImage is null)
                throw new Exception(Messages.ImageTripDetached);

            context.TripImages.Remove(tripImage);
            await context.SaveChangesAsync();

            return (ImageRelationViewModel)tripImage;
        }

        public async Task DeleteImageAsync(Image image)
        {
            var imageId = image.Id;
            Guid imageGuid = image.Guid;
            var createdBy = image.CreatedBy;

            context.Images.Remove(image);
            await context.SaveChangesAsync();

            string fileName = $"{imageGuid}{GetExtensionFromContentType("image/jpeg")}";

            await uploader.DeleteFileAsync(
                config["Firebase:BucketName"]!,
                $"{createdBy}/{fileName}"
            );

            var key = GetImageUpstashKey(imageId);
            await cache.DeleteKeyAsync(key);
        }

        public bool IsOwner(int userId, int imageId)
        {
            var image = context.Images.FirstOrDefault(i =>
                i.Id == imageId && i.CreatedBy == userId
            );

            return image != null;
        }

        private async Task<UrlSigner> GetUrlSignerAsync()
        {
            if (_urlSigner != null)
                return _urlSigner;

            // Get JSON from Key Vault (cached there already)
            var credentialJson = await keyVaultService.GetJsonSecretAsync(
                config["AzureKeyVault:FirebaseKey"]!
            );

            // Parse to GoogleCredential
            var googleCredential = GoogleCredential.FromJson(credentialJson);

            // Create UrlSigner without obsolete API
            _urlSigner = UrlSigner.FromCredential(googleCredential);

            return _urlSigner;
        }

        private async Task<string> GenerateSignedUrlAsync(
            string bucketName,
            string objectName,
            TimeSpan duration
        )
        {
            var urlSigner = await GetUrlSignerAsync();

            return urlSigner.Sign(bucket: bucketName, objectName: objectName, duration: duration);
        }

        private static string GetImageUpstashKey(int id)
        {
            return $"image:{id}:v{CacheVersion.Image_Version}";
        }

        private static string GetExtensionFromContentType(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => ".jpeg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                "image/webp" => ".webp",
                _ => ".bin", // fallback
            };
        }
    }
}
