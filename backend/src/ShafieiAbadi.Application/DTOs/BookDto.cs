namespace ShafieiAbadi.Application.DTOs;

public class BookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string? PublishYear { get; set; }
    public int? PagesCount { get; set; }
    public string? ISBN { get; set; }
    public string? ExternalBuyLink { get; set; }
    public string? DigitalDownloadLink { get; set; }
    public int ViewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public int CommentsCount { get; set; }
}

public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string? PublishYear { get; set; }
    public int? PagesCount { get; set; }
    public string? ISBN { get; set; }
    public string? ExternalBuyLink { get; set; }
    public string? DigitalDownloadLink { get; set; }
    public Guid CategoryId { get; set; }
}

public class UpdateBookDto : CreateBookDto
{
    public Guid Id { get; set; }
}
