using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// مدیریت کتاب‌ها و آثار تالیفی
/// </summary>
public class BooksController : BaseApiController
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// دریافت لیست کتاب‌ها به صورت صفحه‌بندی شده (جهت اسکرول نامحدود)
    /// </summary>
    /// <param name="paginationParams">پارامترهای صفحه‌بندی و فیلتر</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>لیست کتاب‌ها همراه با متادیتا صفحه‌بندی</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<BookDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams, CancellationToken ct)
    {
        var result = await _bookService.GetPagedBooksAsync(paginationParams, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت جزئیات کتاب بر اساس نامک (Slug)
    /// </summary>
    /// <param name="slug">نامک اختصاصی کتاب</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>اطلاعات کامل کتاب به همراه لینک‌های خرید و دانلود</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _bookService.GetBookBySlugAsync(slug, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت جزئیات کتاب بر اساس شناسه (ID)
    /// </summary>
    /// <param name="id">شناسه یکتای کتاب</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>اطلاعات کامل کتاب</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _bookService.GetBookByIdAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// ایجاد کتاب جدید (پنل مدیریت)
    /// </summary>
    /// <param name="dto">اطلاعات کتاب جدید شامل لینک خرید و دانلود</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>کتاب ایجاد شده</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto, CancellationToken ct)
    {
        var result = await _bookService.CreateBookAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// ویرایش کتاب موجود (پنل مدیریت)
    /// </summary>
    /// <param name="id">شناسه کتاب جهت ویرایش</param>
    /// <param name="dto">اطلاعات ویرایش شده کتاب</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>کتاب ویرایش شده</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _bookService.UpdateBookAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف منطقی (Soft Delete) کتاب
    /// </summary>
    /// <param name="id">شناسه کتاب جهت حذف</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات حذف</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _bookService.DeleteBookAsync(id, ct);
        return HandleResult(result);
    }
}
