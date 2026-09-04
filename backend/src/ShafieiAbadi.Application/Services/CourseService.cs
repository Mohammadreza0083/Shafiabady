using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

/// <summary>
/// Service implementation for managing educational video courses.
/// </summary>
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

    public async Task<Result<PagedResult<CourseDto>>> GetPagedAsync(PaginationParams @params, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching courses page {PageNumber} with size {PageSize}", @params.PageNumber, @params.PageSize);

        Expression<Func<Course, bool>> filter = c =>
            !c.IsDeleted &&
            (string.IsNullOrWhiteSpace(@params.Search) || c.Title.Contains(@params.Search) || c.Description.Contains(@params.Search)) &&
            (!@params.CategoryId.HasValue || c.CategoryId == @params.CategoryId.Value) &&
            (string.IsNullOrWhiteSpace(@params.CategorySlug) || @params.CategorySlug == "all" || @params.CategorySlug == "همه" || (c.Category != null && (c.Category.Slug == @params.CategorySlug || c.Category.Title == @params.CategorySlug)));

        var pagedEntities = await _unitOfWork.Repository<Course>().GetPagedAsync(
            @params.PageNumber,
            @params.PageSize,
            filter: filter,
            orderBy: c => c.CreatedAt,
            ascending: false,
            ct: ct,
            includes: new Expression<Func<Course, object>>[] { c => c.Category! });

        var dtos = _mapper.Map<IReadOnlyList<CourseDto>>(pagedEntities.Items);
        var result = new PagedResult<CourseDto>(dtos, pagedEntities.TotalCount, pagedEntities.PageNumber, pagedEntities.PageSize);

        return Result<PagedResult<CourseDto>>.Success(result);
    }

    public async Task<Result<CourseDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving course by slug '{Slug}'", slug);

        var courses = await _unitOfWork.Repository<Course>()
            .FindAsync(c => c.Slug == slug && !c.IsDeleted, ct);

        var course = courses.FirstOrDefault();
        if (course == null)
        {
            _logger.LogWarning("Course with slug '{Slug}' was not found", slug);
            return Result<CourseDto>.NotFound($"Course with slug '{slug}' was not found.");
        }

        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<Result<CourseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id, ct);
        if (course == null || course.IsDeleted)
        {
            _logger.LogWarning("Course with ID '{CourseId}' was not found", id);
            return Result<CourseDto>.NotFound($"Course with ID '{id}' was not found.");
        }

        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<Result<CourseDto>> CreateAsync(CreateCourseDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<CourseDto>.Failure("Course title is required.");

        if (string.IsNullOrWhiteSpace(dto.Slug))
            dto.Slug = dto.Title.Trim().Replace(" ", "-").ToLower();

        var existing = await _unitOfWork.Repository<Course>()
            .FindAsync(c => c.Slug == dto.Slug && !c.IsDeleted, ct);

        if (existing.Any())
            return Result<CourseDto>.Failure($"A course with slug '{dto.Slug}' already exists.");

        var course = _mapper.Map<Course>(dto);
        await _unitOfWork.Repository<Course>().AddAsync(course, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Course '{CourseTitle}' created with ID {CourseId}", course.Title, course.Id);
        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course), 201);
    }

    public async Task<Result<CourseDto>> UpdateAsync(Guid id, UpdateCourseDto dto, CancellationToken ct = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id, ct);
        if (course == null || course.IsDeleted)
            return Result<CourseDto>.NotFound($"Course with ID '{id}' was not found.");

        _mapper.Map(dto, course);
        course.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Course>().Update(course);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Course '{CourseTitle}' (ID {CourseId}) updated successfully", course.Title, course.Id);
        return Result<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id, ct);
        if (course == null || course.IsDeleted)
            return Result.NotFound($"Course with ID '{id}' was not found.");

        _unitOfWork.Repository<Course>().SoftDelete(course);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Course with ID {CourseId} soft-deleted successfully", id);
        return Result.Success();
    }
}
