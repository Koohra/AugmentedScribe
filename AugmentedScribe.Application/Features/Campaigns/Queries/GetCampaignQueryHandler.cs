using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using AugmentedScribe.Domain.Entities;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Queries;

public sealed class GetCampaignQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetCampaignQuery, IEnumerable<CampaignDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ICurrentUserService _currentUserService = currentUserService ??  throw new ArgumentNullException(nameof(currentUserService));

    public async Task<IEnumerable<CampaignDto>> Handle(GetCampaignQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User no authentication");
        }
        
        var campaigns = await _unitOfWork.Campaigns.GetCampaignsByUserIdAsync(userId);

        var campaignDtos = campaigns.Select(campaign => new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            System = campaign.System,
            CreatedAt = campaign.CreatedAt
        }).ToList();
        
        return campaignDtos;
    }
}