using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// مدیریت دوره‌های آموزشی و ویدیوها
/// </summary>
public class CoursesController : BaseApiController
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// دریافت لیست دوره‌ها به صورت صفحه‌بندی شده (جهت اسکرول نامحدود)
    /// </summary>
    /// <param name="paginationParams">پارامترهای صفحه‌بندی و فیلتر</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>لیست دوره‌ها همراه با متادیتا صفحه‌بندی</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<CourseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams, CancellationToken ct)
    {
        var result = await _courseService.GetPagedCoursesAsync(paginationParams, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت جزئیات دوره بر اساس نامک (Slug)
    /// </summary>
    /// <param name="slug">نامک اختصاصی دوره</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>اطلاعات کامل دوره شامل ویدیوی آپارات و سرفصل‌ها</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _courseService.GetCourseBySlugAsync(slug, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت جزئیات دوره بر اساس شناسه (ID)
    /// </summary>
    /// <param name="id">شناسه یکتای دوره</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>اطلاعات کامل دوره</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _courseService.GetCourseByIdAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// ایجاد دوره جدید (پنل مدیریت)
    /// </summary>
    /// <param name="dto">اطلاعات دوره جدید شامل لینک ویدیو آپارات و قیمت‌گذاری</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>دوره ایجاد شده</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken ct)
    {
        var result = await _courseService.CreateCourseAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// ویرایش دوره موجود (پنل مدیریت)
    /// </summary>
    /// <param name="id">شناسه دوره جهت ویرایش</param>
    /// <param name="dto">اطلاعات ویرایش شده دوره</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>دوره ویرایش شده</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _courseService.UpdateCourseAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف منطقی (Soft Delete) دوره
    /// </summary>
    /// <param name="id">شناسه دوره جهت حذف</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات حذف</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _courseService.DeleteCourseAsync(id, ct);
        return HandleResult(result);
    }
}
