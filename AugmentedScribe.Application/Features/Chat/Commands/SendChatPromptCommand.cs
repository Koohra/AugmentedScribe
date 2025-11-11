using AugmentedScribe.Application.Features.Chat.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Chat.Commands;

public sealed record SendChatPromptCommand(
    Guid CampaignId, 
    ChatRequest Request) : IRequest<ChatResponse>;