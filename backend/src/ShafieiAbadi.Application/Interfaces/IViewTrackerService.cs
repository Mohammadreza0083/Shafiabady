using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Interfaces;

public interface IViewTrackerService
{
    Task<Result<bool>> RecordViewAsync(EntityType entityType, Guid entityId, string ipAddress, string? userAgent, CancellationToken ct = default);
    Task<Result<ViewStatsDto>> GetViewStatsAsync(CancellationToken ct = default);
}
