using MediatR;

namespace AugmentedScribe.Application.Features.Books.Commands.DeleteBook;

public record DeleteBookCommand(Guid CampaignId, Guid BookId) : IRequest<Unit>;