using System.ComponentModel.DataAnnotations;
using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Application.Features.Chat.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Chat.Commands;

public sealed class SendChatPromptCommandHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IChatService chatService)
    : IRequestHandler<SendChatPromptCommand, ChatResponse>
{
    private readonly ICurrentUserService _currentUserService =
        currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

    private readonly IUnitOfWork _unitOfWork =
        unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    private readonly IChatService _chatService =
        chatService ?? throw new ArgumentNullException(nameof(chatService));

    public async Task<ChatResponse> Handle(SendChatPromptCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var campaign = await _unitOfWork.Campaigns.GetCampaignByIdAsync(command.CampaignId);
        if (campaign is null)
        {
            throw new ValidationException("Campaign not found.");
        }

        if (campaign.UserId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to access this campaign.");
        }

        var response = await _chatService.GenerateResponseAsync(
            campaign,
            command.Request.Prompt,
            cancellationToken);

        return new ChatResponse
        {
            Response = response
        };
    }
}