using AugmentedScribe.Domain.Enums;

namespace AugmentedScribe.Domain.Entities;

public sealed class Book : Entity
{
    public string FileName { get; init; } = string.Empty;
    public string StorageUrl { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public Guid CampaignId { get; init; }
    public Campaign Campaign { get; init; } = null!;

    private BookStatus _status = BookStatus.Pending;
    public BookStatus Status => _status;
    
    public void StartProcessing()
    {
        if (_status == BookStatus.Pending)
        {
            _status = BookStatus.Processing;
        }
    }
    
    public void MarkAsCompleted()
    {
        if (_status == BookStatus.Processing)
        {
            _status = BookStatus.Completed;
        }
    }
    
    public void MarkAsFailed()
    {
        if (_status != BookStatus.Completed)
        {
            _status = BookStatus.Failed;
        }
    }
}