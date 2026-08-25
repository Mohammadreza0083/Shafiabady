using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// مدیریت مقالات علمی و آموزشی
/// </summary>
public class ArticlesController : BaseApiController
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>
    /// دریافت لیست مقالات به صورت صفحه‌بندی شده (جهت اسکرول نامحدود)
    /// </summary>
    /// <param name="paginationParams">پارامترهای صفحه‌بندی و فیلتر</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>لیست مقالات همراه با متادیتا صفحه‌بندی</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<ArticleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams, CancellationToken ct)
    {
        var result = await _articleService.GetPagedArticlesAsync(paginationParams, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت جزئیات مقاله بر اساس نامک (Slug)
    /// </summary>
    /// <param name="slug">نامک اختصاصی مقاله</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>اطلاعات کامل مقاله</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _articleService.GetArticleBySlugAsync(slug, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// دریافت جزئیات مقاله بر اساس شناسه (ID)
    /// </summary>
    /// <param name="id">شناسه یکتای مقاله</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>اطلاعات کامل مقاله</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _articleService.GetArticleByIdAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// ایجاد مقاله جدید (پنل مدیریت)
    /// </summary>
    /// <param name="dto">اطلاعات مقاله جدید</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>مقاله ایجاد شده</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateArticleDto dto, CancellationToken ct)
    {
        var result = await _articleService.CreateArticleAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// ویرایش مقاله موجود (پنل مدیریت)
    /// </summary>
    /// <param name="id">شناسه مقاله جهت ویرایش</param>
    /// <param name="dto">اطلاعات بروزرسانی شده مقاله</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>مقاله ویرایش شده</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _articleService.UpdateArticleAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// حذف منطقی (Soft Delete) مقاله
    /// </summary>
    /// <param name="id">شناسه مقاله جهت حذف</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات حذف</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _articleService.DeleteArticleAsync(id, ct);
        return HandleResult(result);
    }
}
