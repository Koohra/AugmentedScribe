namespace AugmentedScribe.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICampaignRepository Campaigns { get; }
    IBookRepository Books { get; }
    
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}