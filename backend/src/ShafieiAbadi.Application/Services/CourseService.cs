using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CourseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CourseDto>>> GetPagedCoursesAsync(PaginationParams p, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving courses page {PageNumber} with size {PageSize}", p.PageNumber, p.PageSize);

        Expression<Func<Course, bool>> predicate = c =>
            !c.IsDeleted &&
            (!p.CategoryId.HasValue || c.CategoryId == p.CategoryId.Value) &&
            (string.IsNullOrWhiteSpace(p.SearchTerm) || c.Title.Contains(p.SearchTerm) || c.Description.Contains(p.SearchTerm));

        Func<IQueryable<Course>, IOrderedQueryable<Course>> orderBy = q => q.OrderByDescending(c => c.CreatedAt);

        var (items, totalCount) = await _unitOfWork.Repository<Course>()
            .GetPagedAsync(p.PageNumber, p.PageSize, predicate, orderBy, "Category,Comments", ct);

        var dtos = _mapper.Map<IReadOnlyList<CourseDto>>(items);
        var pagedResult = new PagedResult<CourseDto>(dtos, totalCount, p.PageNumber, p.PageSize);

        return Result<PagedResult<CourseDto>>.Success(pagedResult);
    }

    public async Task<Result<CourseDto>> GetCourseBySlugAsync(string slug, CancellationToken ct = default)
    {
        var courses = await _unitOfWork.Repository<Course>()
            .FindAsync(c => c.Slug == slug && !c.IsDeleted, ct);

        var course = courses.FirstOrDefault();
        if (course == null)
        {
            _logger.LogWarning("Course with slug '{Slug}' was not found", slug);
            return Result<CourseDto>.NotFound($"دوره با نامک '{slug}' یافت نشد.");
        }

        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<Result<CourseDto>> GetCourseByIdAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id, ct);
        if (course == null || course.IsDeleted)
        {
            _logger.LogWarning("Course with Id '{Id}' was not found", id);
            return Result<CourseDto>.NotFound("دوره مورد نظر یافت نشد.");
        }

        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<Result<CourseDto>> CreateCourseAsync(CreateCourseDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<CourseDto>.Failure("عنوان دوره الزامی است.");

        var exists = await _unitOfWork.Repository<Course>()
            .ExistsAsync(c => c.Slug == dto.Slug && !c.IsDeleted, ct);
        if (exists)
            return Result<CourseDto>.Failure("نامک (Slug) وارد شده تکراری است.");

        var course = _mapper.Map<Course>(dto);
        await _unitOfWork.Repository<Course>().AddAsync(course, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Course '{Title}' created successfully with Id {CourseId}", course.Title, course.Id);
        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course), 201);
    }

    public async Task<Result<CourseDto>> UpdateCourseAsync(UpdateCourseDto dto, CancellationToken ct = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(dto.Id, ct);
        if (course == null || course.IsDeleted)
            return Result<CourseDto>.NotFound("دوره جهت ویرایش یافت نشد.");

        _mapper.Map(dto, course);
        course.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Course>().Update(course);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Course {CourseId} updated successfully", course.Id);
        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<Result> DeleteCourseAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id, ct);
        if (course == null || course.IsDeleted)
            return Result.NotFound("دوره جهت حذف یافت نشد.");

        course.IsDeleted = true;
        course.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Course>().Update(course);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Course {CourseId} soft-deleted successfully", id);
        return Result.Success();
    }
}
