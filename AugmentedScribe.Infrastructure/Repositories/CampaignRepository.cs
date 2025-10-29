using AugmentedScribe.Application.Common.Interfaces;
using AugmentedScribe.Domain.Entities;
using AugmentedScribe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AugmentedScribe.Infrastructure.Repositories;

public sealed class CampaignRepository(ScribeDbContext context) : ICampaignRepository
{
    private readonly ScribeDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void AddCampaign(Campaign campaign)
    {
        _context.Campaigns.Add(campaign);
    }

    public async Task<IEnumerable<Campaign>> GetCampaignsByUserIdAsync(string userId)
    {
        return await _context.Campaigns
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Campaign?> GetCampaignByIdAsync(Guid campaignId)
    {
        return await _context.Campaigns.FindAsync(campaignId);
    }
}