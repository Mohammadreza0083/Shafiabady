using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

public class Article : BaseEntity, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string AuthorName { get; set; } = "دکتر محمد حسین شفیع‌آبادی";
    public int ReadTimeMinutes { get; set; } = 5;
    public string Tags { get; set; } = string.Empty;
    public int ViewsCount { get; set; } = 0;
    public bool IsPublished { get; set; } = true;
    public DateTime? PublishedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key & Navigation
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
