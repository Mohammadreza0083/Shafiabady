using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

/// <summary>
/// Represents a classification category for articles, books, and courses.
/// </summary>
public class Category : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Category title or display name.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug identifier for SEO and routing.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Optional short description of the category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display sort order.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Navigation property for associated articles.
    /// </summary>
    public ICollection<Article> Articles { get; set; } = new List<Article>();

    /// <summary>
    /// Navigation property for associated books.
    /// </summary>
    public ICollection<Book> Books { get; set; } = new List<Book>();

    /// <summary>
    /// Navigation property for associated courses.
    /// </summary>
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
