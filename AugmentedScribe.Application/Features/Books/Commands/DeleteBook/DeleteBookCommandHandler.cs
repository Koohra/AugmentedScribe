using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Domain.Exceptions;
using Azure.Storage.Blobs;
using MediatR;

namespace AugmentedScribe.Application.Features.Books.Commands.DeleteBook;

public sealed class DeleteBookCommandHandler(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteBookCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    private readonly IFileStorageService _fileStorageService =
        fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));

    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    public async Task<Unit> Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var campaign = await _unitOfWork.Campaigns.GetCampaignByIdAsync(command.CampaignId);
        if (campaign is null)
        {
            throw new NotFoundException(nameof(campaign), command.CampaignId);
        }

        if (campaign.UserId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to modify this campaign.");
        }

        var book = await _unitOfWork.Books.GetBookByIdAsync(command.BookId, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException(nameof(book), command.BookId);
        }

        if (book.CampaignId != command.CampaignId)
        {
            throw new ValidationException("Book does not belong to this campaign.");
        }

        var blobClient = new BlobClient(new Uri(book.StorageUrl));
        var blobName = blobClient.Name;

        await _fileStorageService.DeleteFileAsync(blobName, cancellationToken);

        _unitOfWork.Books.Delete(book);
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Unit.Value;
    }
}