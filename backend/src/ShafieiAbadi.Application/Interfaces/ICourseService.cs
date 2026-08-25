using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Service contract defining operations for managing and retrieving educational video courses.
/// </summary>
public interface ICourseService
{
    /// <summary>
    /// Retrieves a paginated list of educational courses.
    /// </summary>
    Task<Result<PagedResult<CourseDto>>> GetPagedAsync(PaginationParams @params, CancellationToken ct = default);

    /// <summary>
    /// Retrieves course details, video embed URL, and pricing by URL slug.
    /// </summary>
    Task<Result<CourseDto>> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Retrieves course details by unique identifier.
    /// </summary>
    Task<Result<CourseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new educational course.
    /// </summary>
    Task<Result<CourseDto>> CreateAsync(CreateCourseDto dto, CancellationToken ct = default);

    /// <summary>
    /// Updates course details, pricing, or video URLs.
    /// </summary>
    Task<Result<CourseDto>> UpdateAsync(Guid id, UpdateCourseDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a course by its unique identifier.
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
