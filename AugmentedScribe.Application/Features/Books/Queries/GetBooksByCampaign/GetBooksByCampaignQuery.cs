using AugmentedScribe.Application.Features.Books.Dtos;
using MediatR;

namespace AugmentedScribe.Application.Features.Books.Queries.GetBooksByCampaign;

public sealed record GetBooksByCampaignQuery(Guid CampaignId) : IRequest<IEnumerable<BookDto>>;