using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

public class ArticleService : IArticleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ArticleService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ArticleDto>>> GetPagedArticlesAsync(PaginationParams p, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving articles page {PageNumber} with size {PageSize}", p.PageNumber, p.PageSize);

        Expression<Func<Article, bool>> predicate = a =>
            !a.IsDeleted &&
            a.IsPublished &&
            (!p.CategoryId.HasValue || a.CategoryId == p.CategoryId.Value) &&
            (string.IsNullOrWhiteSpace(p.SearchTerm) || a.Title.Contains(p.SearchTerm) || a.Summary.Contains(p.SearchTerm));

        Func<IQueryable<Article>, IOrderedQueryable<Article>> orderBy = q => q.OrderByDescending(a => a.CreatedAt);

        var (items, totalCount) = await _unitOfWork.Repository<Article>()
            .GetPagedAsync(p.PageNumber, p.PageSize, predicate, orderBy, "Category,Comments", ct);

        var dtos = _mapper.Map<IReadOnlyList<ArticleDto>>(items);
        var pagedResult = new PagedResult<ArticleDto>(dtos, totalCount, p.PageNumber, p.PageSize);

        return Result<PagedResult<ArticleDto>>.Success(pagedResult);
    }

    public async Task<Result<ArticleDto>> GetArticleBySlugAsync(string slug, CancellationToken ct = default)
    {
        var articles = await _unitOfWork.Repository<Article>()
            .FindAsync(a => a.Slug == slug && !a.IsDeleted, ct);

        var article = articles.FirstOrDefault();
        if (article == null)
        {
            _logger.LogWarning("Article with slug '{Slug}' was not found", slug);
            return Result<ArticleDto>.NotFound($"مقاله با نامک '{slug}' یافت نشد.");
        }

        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article));
    }

    public async Task<Result<ArticleDto>> GetArticleByIdAsync(Guid id, CancellationToken ct = default)
    {
        var article = await _unitOfWork.Repository<Article>().GetByIdAsync(id, ct);
        if (article == null || article.IsDeleted)
        {
            _logger.LogWarning("Article with Id '{Id}' was not found", id);
            return Result<ArticleDto>.NotFound("مقاله مورد نظر یافت نشد.");
        }

        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article));
    }

    public async Task<Result<ArticleDto>> CreateArticleAsync(CreateArticleDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<ArticleDto>.Failure("عنوان مقاله الزامی است.");

        var exists = await _unitOfWork.Repository<Article>()
            .ExistsAsync(a => a.Slug == dto.Slug && !a.IsDeleted, ct);
        if (exists)
            return Result<ArticleDto>.Failure("نامک (Slug) وارد شده تکراری است.");

        var article = _mapper.Map<Article>(dto);
        await _unitOfWork.Repository<Article>().AddAsync(article, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Article '{Title}' created successfully with Id {ArticleId}", article.Title, article.Id);
        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article), 201);
    }

    public async Task<Result<ArticleDto>> UpdateArticleAsync(UpdateArticleDto dto, CancellationToken ct = default)
    {
        var article = await _unitOfWork.Repository<Article>().GetByIdAsync(dto.Id, ct);
        if (article == null || article.IsDeleted)
            return Result<ArticleDto>.NotFound("مقاله جهت ویرایش یافت نشد.");

        _mapper.Map(dto, article);
        article.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Article>().Update(article);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Article {ArticleId} updated successfully", article.Id);
        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article));
    }

    public async Task<Result> DeleteArticleAsync(Guid id, CancellationToken ct = default)
    {
        var article = await _unitOfWork.Repository<Article>().GetByIdAsync(id, ct);
        if (article == null || article.IsDeleted)
            return Result.NotFound("مقاله جهت حذف یافت نشد.");

        article.IsDeleted = true;
        article.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Article>().Update(article);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Article {ArticleId} soft-deleted successfully", id);
        return Result.Success();
    }
}
