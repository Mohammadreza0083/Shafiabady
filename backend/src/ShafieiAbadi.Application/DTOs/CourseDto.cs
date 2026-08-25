namespace ShafieiAbadi.Application.DTOs;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? AparatVideoUrl { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int LessonsCount { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public bool IsFree { get; set; }
    public int ViewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public int CommentsCount { get; set; }
}

public class CreateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? AparatVideoUrl { get; set; }
    public string Level { get; set; } = "پیشرفته";
    public string Duration { get; set; } = string.Empty;
    public int LessonsCount { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public bool IsFree { get; set; } = false;
    public Guid CategoryId { get; set; }
}

public class UpdateCourseDto : CreateCourseDto
{
    public Guid Id { get; set; }
}
