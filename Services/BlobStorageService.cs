
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay2.API.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureBlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Blob connection string missing.");

            string containerName =
                configuration["AzureBlobStorage:ContainerName"]
                ?? "raceday-images";

            _container = new BlobContainerClient(
                connectionString,
                containerName);
        }

        public async Task<string> UploadAsync(
            IFormFile file,
            string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was supplied.");

            string extension = Path.GetExtension(file.FileName);

            string blobName =
                $"{folder}/{Guid.NewGuid()}{extension}";

            await _container.CreateIfNotExistsAsync();

            BlobClient blob = _container.GetBlobClient(blobName);

            await using Stream stream = file.OpenReadStream();

            await blob.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blob.Uri.ToString();
        }
    }
}

