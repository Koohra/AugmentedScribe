namespace AugmentedScribe.Domain.Entities;

public sealed class Campaign : Entity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string System { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}