using AugmentedScribe.Domain.Enums;

namespace AugmentedScribe.Domain.Entities;

public sealed class Book : Entity
{
    public string FileName { get; init; } = string.Empty;
    public string StorageUrl { get; init; } = string.Empty;
    public BookStatus Status { get; init; }
    public DateTime UploadedAt { get; init; }
    public Guid CampaignId { get; init; }
    public Campaign Campaign { get; init; } = null!;
}