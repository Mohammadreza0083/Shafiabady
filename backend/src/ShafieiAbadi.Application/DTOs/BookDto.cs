namespace ShafieiAbadi.Application.DTOs;

/// <summary>
/// Data transfer object representing book details in responses.
/// </summary>
public class BookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string PublishYear { get; set; } = string.Empty;
    public int PagesCount { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string? ExternalBuyLink { get; set; }
    public string? DigitalDownloadLink { get; set; }
    public long ViewsCount { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryTitle { get; set; }
    public string? CategorySlug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request payload for creating a new book entry.
/// </summary>
public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string PublishYear { get; set; } = string.Empty;
    public int PagesCount { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string? ExternalBuyLink { get; set; }
    public string? DigitalDownloadLink { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Request payload for updating an existing book entry.
/// </summary>
public class UpdateBookDto : CreateBookDto
{
}
