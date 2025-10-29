using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Domain.Entities;
using AugmentedScribe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AugmentedScribe.Infrastructure.Repositories;

public sealed class BookRepository(ScribeDbContext context) : IBookRepository
{
    private readonly ScribeDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Add(Book book)
    {
        _context.Books.Add(book);
    }

    public void Delete(Book book)
    {
        _context.Books.Remove(book);
    }

    public async Task<IEnumerable<Book>> GetBooksByCampaignIdAsync(Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Books.Where(b => b.CampaignId == campaignId)
            .OrderByDescending(b => b.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetBookByIdAsync(Guid bookId, CancellationToken cancellationToken)
    {
        return await _context.Books.FindAsync([bookId], cancellationToken);
    }
}