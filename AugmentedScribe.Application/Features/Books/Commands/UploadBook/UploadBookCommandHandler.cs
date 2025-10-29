using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Books.Dtos;
using AugmentedScribe.Domain.Entities;
using AugmentedScribe.Domain.Enums;
using MediatR;

namespace AugmentedScribe.Application.Features.Books.Commands.UploadBook;

public sealed class UploadBookCommandHandler(
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadBookCommand, BookDto>
{
    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    private readonly IFileStorageService _fileStorageService =
        fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));

    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<BookDto> Handle(UploadBookCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("No user authentication");
        }

        if (command.File.Length == 0)
        {
            throw new ValidationException("File is required");
        }

        if (command.File.ContentType != "application/pdf")
        {
            throw new ValidationException("File must be pdf");
        }

        var campaign = await _unitOfWork.Campaigns.GetCampaignByIdAsync(command.CampaignId);
        if (campaign is null)
        {
            throw new ValidationException("Campaign not found");
        }

        if (campaign.UserId != userId)
        {
            throw new UnauthorizedAccessException("User not authorized");
        }

        var blobName = $"books/{command.CampaignId}/{Guid.NewGuid()}_{command.File.FileName}";
        string fileUrl;

        await using (var stream = command.File.OpenReadStream())
        {
            fileUrl = await _fileStorageService.UploadFileAsync(stream, blobName, cancellationToken);
        }

        var newBook = new Book
        {
            Id = Guid.NewGuid(),
            FileName = command.File.FileName,
            StorageUrl = fileUrl,
            Status = BookStatus.Pending,
            UploadedAt = DateTime.UtcNow,
            CampaignId = command.CampaignId,
        };

        _unitOfWork.Books.Add(newBook);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new BookDto
        {
            Id = newBook.Id,
            FileName = newBook.FileName,
            StorageUrl = newBook.StorageUrl,
            Status = newBook.Status,
            UploadedAt = newBook.UploadedAt,
            CampaignId = newBook.CampaignId
        };
    }
}