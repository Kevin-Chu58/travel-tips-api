using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using TravelTipsAPI.Clients;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Firebase;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_image;
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

        /// <summary>
        /// Get an iamge by its id
        /// </summary>
        /// <param name="id">image id</param>
        /// <returns>the image with the id</returns>
        public async Task<ImageViewModel> GetImageById(int id)
        {
            // key of the image json
            var key = $"image:{id}:v{CacheVersion.Image_Version}";

            // check cache first, if does not exist, send request to HereMap API
            var cacheJson = await cache.GetAsync(key);
            ImageViewModel imageViewModel;

            if (cacheJson != null)
            {
                // if the cache is not expired, convert it to the correct type
                imageViewModel = JsonSerializer.Deserialize<ImageViewModel>(cacheJson)!;
            }
            else
            {
                var image = context.Images.Find(id);

                if (image is null)
                {
                    throw new Exception(Messages.ImageNotFound);
                }

                imageViewModel = (ImageViewModel)image;

                imageViewModel.Url = GenerateSignedUrlAsync(
                    config["Firebase:BucketName"]!,
                    $"{imageViewModel.CreatedBy}/{image.Guid}.jpeg",
                    TimeSpan.FromDays(7) // Firebase allows maximum 7 days expiration
                ).Result;

                // cache it to UpStash
                string jsonString = JsonSerializer.Serialize(imageViewModel);
                await cache.SetWithExpiryAsync(key, jsonString, Time.WEEK_1);
            }

            return imageViewModel;
        }

        /// <summary>
        /// Get a list of image ids by user id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of image ids</returns>
        public IEnumerable<int> GetImageIdsByUserId(int id)
        {
            var imageIds = context.Images.Where(i => i.CreatedBy == id).Select(i => i.Id).ToList();

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
        /// <param name="stream">image data stream</param>
        /// <param name="contentType">image content type</param>
        /// <param name="userId">user id</param>
        /// <param name="name">image file name</param>
        /// <returns>the posted image</returns>
        public async Task<ImageViewModel> PostNewImageAsync(
            Stream stream,
            string contentType,
            int userId,
            string? name
        )
        {
            try
            {
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
                };

                context.Images.Add(newImage);
                await context.SaveChangesAsync();

                return (ImageViewModel)newImage;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            //catch (Exception)
            //{
            //    throw new Exception(Messages.ImageUploadFailed);
            //}
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
