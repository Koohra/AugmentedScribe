using AugmentedScribe.Application.Features.Campaigns.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Campaigns.Commands;

public sealed class CreateCampaignCommand(CreateCampaignRequest createRequest) : IRequest<CampaignDto>
{
    public CreateCampaignRequest CreateRequest { get; } = createRequest;
}