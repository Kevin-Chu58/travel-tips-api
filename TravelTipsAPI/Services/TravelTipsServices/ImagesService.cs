using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
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
        FirebaseStorageUploader uploader
    ) : IImagesService
    {
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

                // upload image to firebase, throws an exception if failed
                await uploader.UploadFileAsync(
                    stream,
                    contentType,
                    config["Firebase:BucketName"]!,
                    userId.ToString() + "/" + guid.ToString()
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
            catch (ArgumentException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception)
            {
                throw new Exception(Messages.ImageUploadFailed);
            }
        }

        public async Task<ImageRelationViewModel> AttachImageToTrip(int userId, int tripId)
        {
            TripImage newTripImage = new() { TripId = tripId, ImageId = userId };

            context.TripImages.Add(newTripImage);
            await context.SaveChangesAsync();

            return (ImageRelationViewModel)newTripImage;
        }

        public Boolean IsOwner(int userId, int imageId)
        {
            var image = context.Images.FirstOrDefault(i =>
                i.Id == imageId && i.CreatedBy == userId
            );

            return image != null;
        }

        private async Task<string> GenerateSignedUrlAsync(
            string bucketName,
            string objectName,
            TimeSpan duration
        )
        {
            var credential = await keyVaultService.GetJsonSecretAsync(bucketName);
            UrlSigner urlSigner = UrlSigner.FromCredentialFile(credential);

            var signedUrl = urlSigner.Sign(
                bucket: bucketName,
                objectName: objectName,
                duration: duration
            );

            return signedUrl;
        }
    }
}
