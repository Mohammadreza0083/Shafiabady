using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Endpoints for recording entity page views with 24-hour rate limiting and retrieving engagement statistics.
/// </summary>
public class ViewsController : BaseApiController
{
    private readonly IViewTrackerService _viewTrackerService;

    public ViewsController(IViewTrackerService viewTrackerService)
    {
        _viewTrackerService = viewTrackerService;
    }

    /// <summary>
    /// Records an entity view with strict 24-hour client IP rate limiting.
    /// </summary>
    /// <param name="dto">Target entity type and GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Boolean indicating whether view was counted as new (true) or throttled as duplicate (false).</returns>
    [HttpPost("record")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordView([FromBody] RecordViewDto dto, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _viewTrackerService.RecordViewAsync(dto.EntityType, dto.EntityId, ipAddress, userAgent, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves aggregated analytics and top viewed items for the administration dashboard (Admin only).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aggregated view counts, top articles, books, courses, and pending comments counters.</returns>
    [HttpGet("stats")]
    [Authorize]
    [ProducesResponseType(typeof(Result<ViewStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ViewStatsDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _viewTrackerService.GetViewStatsAsync(ct);
        return HandleResult(result);
    }
}
