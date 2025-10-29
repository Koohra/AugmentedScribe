using AugmentedScribe.Domain.Enums;

namespace AugmentedScribe.Application.Features.Books.Dtos;

public sealed record BookDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string StorageUrl { get; init; } = string.Empty;
    public BookStatus Status { get; init; }
    public DateTime UploadedAt { get; init; }
    public Guid CampaignId { get; init; }
}