using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Books.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Books.Queries.GetBooksByCampaign;

public sealed class GetBooksByCampaignQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetBooksByCampaignQuery, IEnumerable<BookDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    public async Task<IEnumerable<BookDto>> Handle(GetBooksByCampaignQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedAccessException("No user authentication");
        }

        var campaign = await _unitOfWork.Campaigns.GetCampaignByIdAsync(query.CampaignId);
        if (campaign is null)
        {
            throw new ValidationException("Campaign not found");
        }

        if (campaign.UserId != userId)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var books = await _unitOfWork.Books.GetBooksByCampaignIdAsync(campaign.Id, cancellationToken);

        var booksDto = books.Select(book => new BookDto
        {
            Id = book.Id,
            FileName = book.FileName,
            StorageUrl = book.StorageUrl,
            Status = book.Status,
            UploadedAt = book.UploadedAt,
            CampaignId = book.CampaignId
        }).ToList();

        return booksDto;
    }
}