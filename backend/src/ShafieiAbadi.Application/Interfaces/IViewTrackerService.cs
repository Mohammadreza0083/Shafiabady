using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Service contract defining operations for recording entity views and tracking engagement metrics.
/// </summary>
public interface IViewTrackerService
{
    /// <summary>
    /// Records a view for an entity while enforcing the strict 24-hour rate limit per client IP address.
    /// </summary>
    Task<Result<bool>> RecordViewAsync(EntityType entityType, Guid entityId, string ipAddress, string? userAgent = null, CancellationToken ct = default);

    /// <summary>
    /// Computes aggregated platform analytics and top viewed items for the administration dashboard.
    /// </summary>
    Task<Result<ViewStatsDto>> GetViewStatsAsync(CancellationToken ct = default);
}
