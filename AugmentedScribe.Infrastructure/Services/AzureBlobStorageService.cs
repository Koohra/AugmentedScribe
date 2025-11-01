using AugmentedScribe.Application.Common.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace AugmentedScribe.Infrastructure.Services;

public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
                               ?? throw new InvalidOperationException(
                                   "AzureStorage:ConnectionString is not configured in appsettings.Development.json");
        var containerName = configuration["AzureStorage:ContainerName"] ??
                            throw new InvalidOperationException(
                                "AzureStorage:ContainerName is not configured in appsettings.Development.json");

        var blobServiceClient = new BlobServiceClient(connectionString);

        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string blobName,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken: cancellationToken);
        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, cancellationToken);
    }

    public async Task<Stream> DownloadFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            throw new FileNotFoundException("Blob not found", blobName);
        }

        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream, cancellationToken);
        stream.Position = 0;
        return stream;
    }
}