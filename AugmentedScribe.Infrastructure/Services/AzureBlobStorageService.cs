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
        var connectionString = configuration["AzureBlobStorage:ConnectionString"]
                               ?? throw new InvalidOperationException(
                                   "AzureStorage:ConnectionString is not configured in appsettings.Development.json");
        var containerName = configuration["AzureBlobStorage:ContainerName"] ??
                            throw new InvalidOperationException(
                                "AzureStorage:ContainerName is not configured in appsettings.Development.json");

        var blobServiceClient = new BlobServiceClient(connectionString);

        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string blobName,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken: cancellationToken);
        return blobClient.Uri.ToString();
    }
}