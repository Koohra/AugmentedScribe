using AugmentedScribe.Domain.Entities;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface IBookRepository
{
    void Add(Book book);
    Task<IEnumerable<Book>> GetBooksByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default);
}