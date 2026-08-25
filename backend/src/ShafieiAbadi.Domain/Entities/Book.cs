using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

/// <summary>
/// Represents a published academic book, textbook, or monograph.
/// </summary>
public class Book : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Book title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Unique URL slug for SEO routing.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Optional book subtitle.
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Detailed description or synopsis.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL to the book cover thumbnail image.
    /// </summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// Publishing house or university press name.
    /// </summary>
    public string Publisher { get; set; } = string.Empty;

    /// <summary>
    /// Year of publication (Persian or Gregorian calendar).
    /// </summary>
    public string PublishYear { get; set; } = string.Empty;

    /// <summary>
    /// Total page count.
    /// </summary>
    public int PagesCount { get; set; } = 0;

    /// <summary>
    /// International Standard Book Number (ISBN).
    /// </summary>
    public string ISBN { get; set; } = string.Empty;

    /// <summary>
    /// External hyperlink for purchasing physical copies.
    /// </summary>
    public string? ExternalBuyLink { get; set; }

    /// <summary>
    /// Digital download link for sample chapters or PDF version.
    /// </summary>
    public string? DigitalDownloadLink { get; set; }

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
    /// Navigation collection of reader reviews and comments.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
