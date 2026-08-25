using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// آمار و رهگیری بازدیدها بر اساس آی‌پی (قانون ۲۴ ساعت)
/// </summary>
public class ViewsController : BaseApiController
{
    private readonly IViewTrackerService _viewTrackerService;

    public ViewsController(IViewTrackerService viewTrackerService)
    {
        _viewTrackerService = viewTrackerService;
    }

    /// <summary>
    /// ثبت بازدید یک موجودیت بر اساس آدرس IP (حداکثر ۱ بار در هر ۲۴ ساعت)
    /// </summary>
    /// <param name="dto">اطلاعات نوع و شناسه موجودیت</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>نتیجه ثبت بازدید (آیا بازدید جدید ثبت شد یا خیر)</returns>
    [HttpPost("record")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordView([FromBody] RecordViewDto dto, CancellationToken ct)
    {
        var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? HttpContext.Connection.RemoteIpAddress?.ToString()
            ?? "127.0.0.1";

        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var result = await _viewTrackerService.RecordViewAsync(dto.EntityType, dto.EntityId, ipAddress, userAgent, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت آمار کلی و برترین‌های سایت جهت داشبورد مدیریت
    /// </summary>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>مجموع بازدیدها و پربازدیدترین مقاله، کتاب و دوره</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(Result<ViewStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _viewTrackerService.GetViewStatsAsync(ct);
        return HandleResult(result);
    }
}
