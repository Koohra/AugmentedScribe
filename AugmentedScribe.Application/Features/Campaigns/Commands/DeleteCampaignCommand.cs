using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Commands;

public sealed record DeleteCampaignCommand(Guid CampaignId) : IRequest<Unit>;