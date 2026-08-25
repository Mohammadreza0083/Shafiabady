using ShafieiAbadi.Domain.Common;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Domain.Entities;

/// <summary>
/// Audit log entry to enforce the 24-hour rate limit per entity per client IP address.
/// </summary>
public class ViewLog : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Type of entity viewed (Article, Book, Course).
    /// </summary>
    public EntityType EntityType { get; set; }

    /// <summary>
    /// Identifier of the target entity.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Client IPv4 or IPv6 address.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Optional HTTP User-Agent header string for device analytics.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// UTC timestamp of the view record.
    /// </summary>
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks whether this view occurred within 24 hours relative to a specified reference point.
    /// </summary>
    /// <param name="relativeTo">Optional reference UTC timestamp (defaults to DateTime.UtcNow).</param>
    /// <returns>True if viewed within 24 hours before the reference timestamp.</returns>
    public bool IsWithin24Hours(DateTime? relativeTo = null)
    {
        var targetTime = relativeTo ?? DateTime.UtcNow;
        return ViewedAt >= targetTime.AddHours(-24);
    }
}
