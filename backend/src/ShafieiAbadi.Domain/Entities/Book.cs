using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

public class Book : BaseEntity, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string? PublishYear { get; set; }
    public int? PagesCount { get; set; }
    public string? ISBN { get; set; }
    public string? ExternalBuyLink { get; set; }
    public string? DigitalDownloadLink { get; set; }
    public int ViewsCount { get; set; } = 0;

    // Foreign Key & Navigation
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
