namespace ShafieiAbadi.Application.DTOs;

/// <summary>
/// Data transfer object representing category details and content counters.
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public int ArticlesCount { get; set; }
    public int BooksCount { get; set; }
    public int CoursesCount { get; set; }
}

/// <summary>
/// Request payload for creating a new content category.
/// </summary>
public class CreateCategoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
}
