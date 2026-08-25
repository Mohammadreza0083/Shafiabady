using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

public class Course : BaseEntity, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? AparatVideoUrl { get; set; }
    public string Level { get; set; } = "پیشرفته";
    public string Duration { get; set; } = string.Empty;
    public int LessonsCount { get; set; } = 0;
    public decimal OriginalPrice { get; set; } = 0;
    public decimal? DiscountedPrice { get; set; }
    public int? DiscountPercentage { get; set; }
    public bool IsFree { get; set; } = false;
    public int ViewsCount { get; set; } = 0;

    // Foreign Key & Navigation
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
