using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

/// <summary>
/// Represents a published article or academic post.
/// </summary>
public class Article : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Article title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Unique URL slug for SEO routing.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Short summary or excerpt.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Rich HTML content of the article.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// URL to the featured thumbnail image.
    /// </summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// Author name (defaults strictly to "دکتر محمد حسین شفیع‌آبادی").
    /// </summary>
    public string AuthorName { get; set; } = "دکتر محمد حسین شفیع‌آبادی";

    /// <summary>
    /// Estimated reading time in minutes.
    /// </summary>
    public int ReadTimeMinutes { get; set; } = 5;

    /// <summary>
    /// Comma-separated search tags or keywords.
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the article is published and visible to the public.
    /// </summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// Total cumulative view count.
    /// </summary>
    public long ViewsCount { get; set; } = 0;

    /// <summary>
    /// Foreign key referencing the associated Category.
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Navigation property to the Category.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Navigation collection of user comments.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
