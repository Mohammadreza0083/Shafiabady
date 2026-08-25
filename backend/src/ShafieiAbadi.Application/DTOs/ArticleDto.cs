namespace ShafieiAbadi.Application.DTOs;

public class ArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int ReadTimeMinutes { get; set; }
    public string Tags { get; set; } = string.Empty;
    public int ViewsCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public int CommentsCount { get; set; }
}

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
    public Guid CategoryId { get; set; }
    public bool IsPublished { get; set; } = true;
}

public class UpdateArticleDto : CreateArticleDto
{
    public Guid Id { get; set; }
}
