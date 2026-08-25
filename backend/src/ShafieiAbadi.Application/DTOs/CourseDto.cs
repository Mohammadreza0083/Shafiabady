namespace ShafieiAbadi.Application.DTOs;

/// <summary>
/// Data transfer object representing training course details in responses.
/// </summary>
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
    public long ViewsCount { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public string? CategorySlug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request payload for creating a new training course.
/// </summary>
public class CreateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? AparatVideoUrl { get; set; }
    public string Level { get; set; } = "متوسط";
    public string Duration { get; set; } = string.Empty;
    public int LessonsCount { get; set; } = 0;
    public decimal OriginalPrice { get; set; } = 0;
    public decimal? DiscountedPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Request payload for updating an existing training course.
/// </summary>
public class UpdateCourseDto : CreateCourseDto
{
}
