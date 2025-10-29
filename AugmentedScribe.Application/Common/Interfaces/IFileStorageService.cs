namespace AugmentedScribe.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string blobName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string blobName, CancellationToken cancellationToken = default);
}