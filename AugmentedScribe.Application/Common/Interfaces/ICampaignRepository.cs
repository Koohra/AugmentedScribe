using AugmentedScribe.Domain.Entities;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface ICampaignRepository
{
    void AddCampaign(Campaign campaign);
    
    Task<IEnumerable<Campaign>> GetCampaignsByUserIdAsync(string userId);
}