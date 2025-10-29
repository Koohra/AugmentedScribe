using AugmentedScribe.Application.Features.Campaigns.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Queries;

public record GetCampaignQuery : IRequest<IEnumerable<CampaignDto>>;