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
    /// Retrieves a category by its unique GUID.
    /// </summary>
    /// <param name="id">Unique category GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Category details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, ct);
        if (category == null || category.IsDeleted)
        {
            return HandleResult(Result<CategoryDto>.NotFound("دسته‌بندی مورد نظر یافت نشد."));
        }

        return HandleResult(Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category)));
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

    /// <summary>
    /// Updates an existing category (Admin only).
    /// </summary>
    /// <param name="id">Unique category GUID.</param>
    /// <param name="dto">Updated category data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated category details.</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, ct);
        if (category == null || category.IsDeleted)
        {
            return HandleResult(Result<CategoryDto>.NotFound("دسته‌بندی مورد نظر یافت نشد."));
        }

        category.Title = dto.Title;
        category.Slug = dto.Slug;
        category.Description = dto.Description;
        category.DisplayOrder = dto.DisplayOrder;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return HandleResult(Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category)));
    }

    /// <summary>
    /// Soft deletes a category (Admin only).
    /// </summary>
    /// <param name="id">Unique category GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standard success result.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, ct);
        if (category == null || category.IsDeleted)
        {
            return HandleResult(Result.NotFound("دسته‌بندی مورد نظر یافت نشد."));
        }

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return HandleResult(Result.Success());
    }
}
