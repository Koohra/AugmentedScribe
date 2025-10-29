using AugmentedScribe.Application.Features.Campaigns.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Queries;

public record GetCampaignByIdQuery(Guid CampaignId) : IRequest<CampaignDto>;