namespace ShafieiAbadi.Application.DTOs;

/// <summary>
/// Data transfer object representing article details in responses.
/// </summary>
public class ArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string AuthorName { get; set; } = "دکتر محمد حسین شفیع‌آبادی";
    public int ReadTimeMinutes { get; set; }
    public string Tags { get; set; } = string.Empty;
    public long ViewsCount { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public string? CategorySlug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request payload for creating a new article.
/// </summary>
public class CreateArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string AuthorName { get; set; } = "دکتر محمد حسین شفیع‌آبادی";
    public int ReadTimeMinutes { get; set; } = 5;
    public string Tags { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Request payload for updating an existing article.
/// </summary>
public class UpdateArticleDto : CreateArticleDto
{
}
