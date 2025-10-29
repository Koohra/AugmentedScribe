using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Commands;

public sealed record UpdateCampaignCommand(Guid CampaignId, string Name, string? Description, string System) : IRequest<Unit>;