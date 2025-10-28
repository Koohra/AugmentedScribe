using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Infrastructure.Repositories;

namespace AugmentedScribe.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ScribeDbContext _context;

    public ICampaignRepository Campaigns { get; }

    public UnitOfWork(ScribeDbContext context)
    {
        _context = context;
        Campaigns = new CampaignRepository(_context);
    }


    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}