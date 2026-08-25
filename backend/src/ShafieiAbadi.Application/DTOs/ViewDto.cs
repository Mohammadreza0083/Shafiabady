using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.DTOs;

public class RecordViewDto
{
    public EntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class ViewStatsDto
{
    public long TotalViews { get; set; }
    public ArticleDto? TopArticle { get; set; }
    public BookDto? TopBook { get; set; }
    public CourseDto? TopCourse { get; set; }
    public int PendingCommentsCount { get; set; }
}
