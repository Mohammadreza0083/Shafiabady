using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Endpoints for managing, browsing, and querying educational video courses, Aparat video embeds, and course pricing.
/// </summary>
public class CoursesController : BaseApiController
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// Retrieves a paginated list of educational courses to support infinite scrolling and archive filtering.
    /// </summary>
    /// <param name="paginationParams">Filtering, search, and pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated course collection with total counts and pagination metadata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<CourseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams, CancellationToken ct)
    {
        var result = await _courseService.GetPagedAsync(paginationParams, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves full course details, Aparat video player URLs, and pricing by unique URL slug.
    /// </summary>
    /// <param name="slug">Unique URL slug identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed course data payload.</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _courseService.GetBySlugAsync(slug, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves full course details by its unique identifier (GUID).
    /// </summary>
    /// <param name="id">Unique course GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed course data payload.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _courseService.GetByIdAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new educational course (Admin only).
    /// </summary>
    /// <param name="dto">Course creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created course details.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken ct)
    {
        var result = await _courseService.CreateAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing course's metadata, pricing, or video URLs (Admin only).
    /// </summary>
    /// <param name="id">Unique course GUID to update.</param>
    /// <param name="dto">Updated course payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated course details.</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<CourseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
    {
        var result = await _courseService.UpdateAsync(id, dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Soft-deletes a course by its unique identifier (Admin only).
    /// </summary>
    /// <param name="id">Unique course GUID to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result indicating success or failure.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _courseService.DeleteAsync(id, ct);
        return HandleResult(result);
    }
}
