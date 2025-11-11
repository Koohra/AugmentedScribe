using AugmentedScribe.Domain.Entities;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface IChatService
{
    Task<string> GenerateResponseAsync(
        Campaign campaign,
        string prompt,
        CancellationToken cancellationToken = default);
}