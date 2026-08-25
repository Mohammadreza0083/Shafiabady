using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

/// <summary>
/// Represents an educational video training course.
/// </summary>
public class Course : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Course title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Unique URL slug for SEO routing.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Detailed course syllabus and overview.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL to the course cover thumbnail image.
    /// </summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL or embed link to the Aparat preview video.
    /// </summary>
    public string? AparatVideoUrl { get; set; }

    /// <summary>
    /// Difficulty level (e.g., Introductory, Intermediate, Advanced).
    /// </summary>
    public string Level { get; set; } = "متوسط";

    /// <summary>
    /// Total duration string (e.g., "12 hours").
    /// </summary>
    public string Duration { get; set; } = string.Empty;

    /// <summary>
    /// Total number of video lectures or chapters.
    /// </summary>
    public int LessonsCount { get; set; } = 0;

    /// <summary>
    /// Original price in Tomans.
    /// </summary>
    public decimal OriginalPrice { get; set; } = 0;

    /// <summary>
    /// Discounted sale price in Tomans.
    /// </summary>
    public decimal? DiscountedPrice { get; set; }

    /// <summary>
    /// Percentage discount applied to original price.
    /// </summary>
    public int? DiscountPercentage { get; set; }

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
    /// Navigation collection of student reviews and comments.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
