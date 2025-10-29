using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Domain.Entities;
using AugmentedScribe.Infrastructure.Persistence;

namespace AugmentedScribe.Infrastructure.Repositories;

public sealed class BookRepository(ScribeDbContext context) : IBookRepository
{
    private readonly ScribeDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Add(Book book)
    {
        _context.Books.Add(book);
    }
}