using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Commands;

public sealed class UpdateCampaignCommandHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCampaignCommand, Unit>
{
    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<Unit> Handle(UpdateCampaignCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("No user authentication");
        }

        var campaign = await _unitOfWork.Campaigns.GetCampaignByIdAsync(command.CampaignId);
        if (campaign is null)
        {
            throw new ValidationException("Campaign not found");
        }

        if (campaign.UserId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this campaign.");
        }

        campaign.Name = command.Name;
        campaign.Description = command.Description;
        campaign.System = command.System;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Unit.Value;
    }
}