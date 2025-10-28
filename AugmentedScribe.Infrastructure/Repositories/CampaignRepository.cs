using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Domain.Entities;
using AugmentedScribe.Infrastructure.Persistence;

namespace AugmentedScribe.Infrastructure.Repositories;

public sealed class CampaignRepository(ScribeDbContext context) : ICampaignRepository
{
    private readonly ScribeDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    
    public void AddCampaign(Campaign campaign)
    {
        _context.Campaigns.Add(campaign);
    }
}