using AugmentedScribe.Domain.Entities;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface ICampaignRepository
{
    void AddCampaign(Campaign campaign);
}