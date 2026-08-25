using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// مدیریت دیدگاه‌ها و پاسخ‌های تو در تو
/// </summary>
public class CommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// دریافت درخت سلسله‌مراتبی دیدگاه‌های تایید شده برای یک مقاله، کتاب یا دوره
    /// </summary>
    /// <param name="articleId">شناسه مقاله (اختیاری)</param>
    /// <param name="bookId">شناسه کتاب (اختیاری)</param>
    /// <param name="courseId">شناسه دوره (اختیاری)</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>درخت سلسله‌مراتبی دیدگاه‌ها با پاسخ‌های نامحدود</returns>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(Result<List<CommentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree([FromQuery] Guid? articleId, [FromQuery] Guid? bookId, [FromQuery] Guid? courseId, CancellationToken ct)
    {
        var result = await _commentService.GetApprovedCommentsTreeAsync(articleId, bookId, courseId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت کلیه دیدگاه‌ها جهت صف بررسی و تایید مدیر (پنل مدیریت)
    /// </summary>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">تعداد در هر صفحه</param>
    /// <param name="status">فیلتر وضعیت دیدگاه (در انتظار، تایید شده، رد شده)</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>لیست دیدگاه‌ها همراه با متادیتا</returns>
    [HttpGet("admin/paged")]
    [ProducesResponseType(typeof(Result<PagedResult<CommentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 15, [FromQuery] CommentStatus? status = null, CancellationToken ct = default)
    {
        var result = await _commentService.GetAllCommentsPagedAsync(pageNumber, pageSize, status, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// ثبت دیدگاه جدید توسط کاربر یا دانشجو (نیازمند تایید مدیر)
    /// </summary>
    /// <param name="dto">اطلاعات دیدگاه یا پاسخ</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>دیدگاه ثبت شده در وضعیت در انتظار</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CommentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto, CancellationToken ct)
    {
        var result = await _commentService.CreateCommentAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// تغییر وضعیت دیدگاه (تایید یا رد دیدگاه توسط مدیر)
    /// </summary>
    /// <param name="dto">شناسه دیدگاه و وضعیت جدید</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات تغییر وضعیت</returns>
    [HttpPut("status")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateCommentStatusDto dto, CancellationToken ct)
    {
        var result = await _commentService.UpdateCommentStatusAsync(dto.CommentId, dto.Status, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف دیدگاه
    /// </summary>
    /// <param name="id">شناسه دیدگاه</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات حذف</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _commentService.DeleteCommentAsync(id, ct);
        return HandleResult(result);
    }
}
