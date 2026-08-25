using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.DTOs;

/// <summary>
/// Request payload for recording an entity view.
/// </summary>
public class RecordViewDto
{
    public EntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
}

/// <summary>
/// Aggregated view counts and top content statistics for the administration dashboard.
/// </summary>
public class ViewStatsDto
{
    public long TotalViews { get; set; }
    public long TotalArticlesViews { get; set; }
    public long TotalBooksViews { get; set; }
    public long TotalCoursesViews { get; set; }
    public string TopArticleTitle { get; set; } = string.Empty;
    public long TopArticleViews { get; set; }
    public string TopBookTitle { get; set; } = string.Empty;
    public long TopBookViews { get; set; }
    public string TopCourseTitle { get; set; } = string.Empty;
    public long TopCourseViews { get; set; }
    public int PendingCommentsCount { get; set; }
}
