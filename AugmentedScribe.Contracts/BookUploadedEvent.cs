namespace AugmentedScribe.Contracts;

public record BookUploadedEvent(
    Guid BookId,
    Guid CampaignId,
    string StorageUrl);