using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace TravelTipsAPI.Firebase
{
    public class FirebaseStorageUploader
    {
        private readonly StorageClient _storageClient;

        public FirebaseStorageUploader(string json)
        {
            var credential = GoogleCredential.FromJson(json);
            // Create once, reuse forever
            _storageClient = StorageClient.CreateAsync(credential).GetAwaiter().GetResult();
        }

        public async Task UploadFileAsync(
            Stream fileStream,
            string contentType,
            string bucketName,
            string objectName
        )
        {
            try
            {
                await _storageClient.UploadObjectAsync(
                    bucketName,
                    objectName,
                    contentType,
                    fileStream
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Upload Failed", ex);
            }
        }

        public async Task DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                await _storageClient.DeleteObjectAsync(bucketName, objectName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete file", ex);
            }
        }
    }
}
