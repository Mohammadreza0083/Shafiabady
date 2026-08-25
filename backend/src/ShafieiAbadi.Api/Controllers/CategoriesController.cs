using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Endpoints for managing and querying content taxonomy and categories.
/// </summary>
public class CategoriesController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves all active categories along with counts of associated articles, books, and courses.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of category summaries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var pagedCategories = await _unitOfWork.Repository<Category>()
            .GetPagedAsync(1, 100, c => !c.IsDeleted, c => c.DisplayOrder, true, ct, c => c.Articles, c => c.Books, c => c.Courses);

        var dtos = _mapper.Map<IReadOnlyList<CategoryDto>>(pagedCategories.Items);
        return HandleResult(Result<IReadOnlyList<CategoryDto>>.Success(dtos));
    }

    /// <summary>
    /// Creates a new category classification (Admin only).
    /// </summary>
    /// <param name="dto">Category creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created category details.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var category = _mapper.Map<Category>(dto);
        await _unitOfWork.Repository<Category>().AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return HandleResult(Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category), 201));
    }
}
