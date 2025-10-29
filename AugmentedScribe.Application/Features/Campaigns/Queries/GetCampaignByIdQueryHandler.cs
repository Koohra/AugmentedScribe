using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Queries;

public sealed class GetCampaignByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetCampaignByIdQuery, CampaignDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    public async Task<CampaignDto> Handle(GetCampaignByIdQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User not authorized");
        }

        var campaign = await _unitOfWork.Campaigns.GetCampaignByIdAsync(query.CampaignId);
        if (campaign is null)
        {
            throw new ValidationException("Campaign not found");
        }

        if (campaign.UserId != userId)
        {
            throw new UnauthorizedAccessException("User not authorized");
        }

        var campaignDto = new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            System = campaign.System,
            CreatedAt = campaign.CreatedAt
        };

        return campaignDto;
    }
}