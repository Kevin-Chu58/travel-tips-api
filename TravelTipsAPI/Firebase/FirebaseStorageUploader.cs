using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace TravelTipsAPI.Firebase
{
    public class FirebaseStorageUploader(string json)
    {
        private readonly GoogleCredential credential = GoogleCredential.FromJson(json);

        public async Task UploadFileAsync(
            Stream fileStream,
            string contentType,
            string bucketName,
            string objectName
        )
        {
            try
            {
                var storageClient = await StorageClient.CreateAsync(credential);

                await storageClient.UploadObjectAsync(
                    bucketName,
                    objectName,
                    contentType,
                    fileStream
                );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                var storageClient = await StorageClient.CreateAsync(credential);
                await storageClient.DeleteObjectAsync(bucketName, objectName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete file: {ex.Message}");
            }
        }
    }
}
