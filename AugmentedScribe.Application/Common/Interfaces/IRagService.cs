using AugmentedScribe.Domain.Entities;

namespace AugmentedScribe.Application.Common.Interfaces;

public interface IRagService
{
    Task GenerateEmbeddingsAsync(Book book, string text, CancellationToken cancellationToken = default);
}