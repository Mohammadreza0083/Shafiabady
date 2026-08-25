using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

public interface ICourseService
{
    Task<Result<PagedResult<CourseDto>>> GetPagedCoursesAsync(PaginationParams paginationParams, CancellationToken ct = default);
    Task<Result<CourseDto>> GetCourseBySlugAsync(string slug, CancellationToken ct = default);
    Task<Result<CourseDto>> GetCourseByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CourseDto>> CreateCourseAsync(CreateCourseDto dto, CancellationToken ct = default);
    Task<Result<CourseDto>> UpdateCourseAsync(UpdateCourseDto dto, CancellationToken ct = default);
    Task<Result> DeleteCourseAsync(Guid id, CancellationToken ct = default);
}
