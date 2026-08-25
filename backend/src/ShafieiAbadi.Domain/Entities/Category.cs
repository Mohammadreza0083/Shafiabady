using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

public class Category : BaseEntity, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Book> Books { get; set; } = new List<Book>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
