using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Campaigns.Dtos;
using AugmentedScribe.Domain.Entities;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Commands;

public sealed class CreateCampaignCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateCampaignCommand, CampaignDto>
{
    private readonly IUnitOfWork _unitOfWork =
        unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    public async Task<CampaignDto> Handle(CreateCampaignCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("No user authentication");
        }

        var request = command.CreateRequest;
        var newCampaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            System = request.System,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _unitOfWork.Campaigns.AddCampaign(newCampaign);
        await _unitOfWork.CompleteAsync(cancellationToken);

        var campaignDto = new CampaignDto
        {
            Id = newCampaign.Id,
            Name = newCampaign.Name,
            Description = newCampaign.Description,
            System = newCampaign.System,
            CreatedAt = newCampaign.CreatedAt,
        };

        return campaignDto;
    }
}