using AugmentedScribe.Domain.Entities;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface IBookRepository
{
    void Add(Book book);
    void Delete(Book book);
    Task<IEnumerable<Book>> GetBooksByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task<Book?> GetBookByIdAsync(Guid bookId, CancellationToken cancellationToken = default);
}