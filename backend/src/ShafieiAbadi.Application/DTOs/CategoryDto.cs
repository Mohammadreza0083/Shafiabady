namespace ShafieiAbadi.Application.DTOs;

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

public class CreateCategoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
