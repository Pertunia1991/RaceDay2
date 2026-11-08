
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay2.API.Services
{
    public class AzureBlobService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            var containerName =
                configuration["AzureStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "Azure Storage container name is missing.");
            }

            var blobServiceClient =
                new BlobServiceClient(connectionString);

            _containerClient =
                blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadImageAsync(
            Stream fileStream,
            string fileName,
            string contentType)
        {
            await _containerClient.CreateIfNotExistsAsync(
                PublicAccessType.Blob);

            var uniqueFileName =
                $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

            var blobClient =
                _containerClient.GetBlobClient(uniqueFileName);

            await blobClient.UploadAsync(
                fileStream,
                new BlobHttpHeaders
                {
                    ContentType = contentType
                });

            return blobClient.Uri.ToString();
        }
    }
}
