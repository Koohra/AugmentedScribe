using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Commands;

public sealed class DeleteCampaignCommandHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCampaignCommand, Unit>
{
    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<Unit> Handle(DeleteCampaignCommand command, CancellationToken cancellationToken)
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

        _unitOfWork.Campaigns.DeleteCampaign(campaign);
        await _unitOfWork.CompleteAsync(cancellationToken);
        return Unit.Value;
    }
}