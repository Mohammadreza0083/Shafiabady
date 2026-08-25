using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Endpoints for publishing, querying, and managing scholarly and technical articles.
/// </summary>
public class ArticlesController : BaseApiController
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>
    /// Retrieves a paginated list of articles to support infinite scrolling and archive filtering.
    /// </summary>
    /// <param name="paginationParams">Filtering, search, and pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated article collection with total item counts and page indicators.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<ArticleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams, CancellationToken ct)
    {
        var result = await _articleService.GetPagedAsync(paginationParams, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves full article details by its unique URL slug.
    /// </summary>
    /// <param name="slug">Unique URL slug identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed article data payload including category and metadata.</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _articleService.GetBySlugAsync(slug, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves full article details by its unique identifier (GUID).
    /// </summary>
    /// <param name="id">Unique article GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed article data payload.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _articleService.GetByIdAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates and publishes a new article (Admin only).
    /// </summary>
    /// <param name="dto">Article creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created article details.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateArticleDto dto, CancellationToken ct)
    {
        var result = await _articleService.CreateAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing article's metadata and content (Admin only).
    /// </summary>
    /// <param name="id">Unique article GUID to update.</param>
    /// <param name="dto">Updated article payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated article details.</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ArticleDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleDto dto, CancellationToken ct)
    {
        var result = await _articleService.UpdateAsync(id, dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Soft-deletes an article by its unique identifier (Admin only).
    /// </summary>
    /// <param name="id">Unique article GUID to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result indicating success or failure.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _articleService.DeleteAsync(id, ct);
        return HandleResult(result);
    }
}
