using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// مدیریت دسته‌بندی‌ها
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
    /// دریافت تمامی دسته‌بندی‌ها به همراه تعداد مقالات، کتاب‌ها و دوره‌ها
    /// </summary>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>لیست کامل دسته‌بندی‌ها</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var (items, _) = await _unitOfWork.Repository<Category>()
            .GetPagedAsync(1, 100, c => !c.IsDeleted, q => q.OrderBy(c => c.DisplayOrder), "Articles,Books,Courses", ct);

        var dtos = _mapper.Map<IReadOnlyList<CategoryDto>>(items);
        return HandleResult(Result<IReadOnlyList<CategoryDto>>.Success(dtos));
    }

    /// <summary>
    /// ایجاد دسته‌بندی جدید
    /// </summary>
    /// <param name="dto">اطلاعات دسته‌بندی</param>
    /// <param name="ct">توکن لغو عملیات</param>
    /// <returns>دسته‌بندی ایجاد شده</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var category = _mapper.Map<Category>(dto);
        await _unitOfWork.Repository<Category>().AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return HandleResult(Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category), 201));
    }
}
