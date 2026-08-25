using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

/// <summary>
/// Service implementation for managing articles.
/// </summary>
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

    public async Task<Result<PagedResult<ArticleDto>>> GetPagedAsync(PaginationParams @params, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching articles page {PageNumber} with page size {PageSize}", @params.PageNumber, @params.PageSize);

        Expression<Func<Article, bool>> filter = a =>
            !a.IsDeleted &&
            (string.IsNullOrWhiteSpace(@params.Search) || a.Title.Contains(@params.Search) || a.Summary.Contains(@params.Search) || a.Tags.Contains(@params.Search)) &&
            (string.IsNullOrWhiteSpace(@params.CategorySlug) || (a.Category != null && a.Category.Slug == @params.CategorySlug)) &&
            (string.IsNullOrWhiteSpace(@params.Tag) || a.Tags.Contains(@params.Tag));

        var pagedEntities = await _unitOfWork.Repository<Article>().GetPagedAsync(
            @params.PageNumber,
            @params.PageSize,
            filter: filter,
            orderBy: a => a.CreatedAt,
            ascending: false,
            ct: ct,
            includes: new Expression<Func<Article, object>>[] { a => a.Category! });

        var dtos = _mapper.Map<IReadOnlyList<ArticleDto>>(pagedEntities.Items);
        var result = new PagedResult<ArticleDto>(dtos, pagedEntities.TotalCount, pagedEntities.PageNumber, pagedEntities.PageSize);

        return Result<PagedResult<ArticleDto>>.Success(result);
    }

    public async Task<Result<ArticleDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving article by slug '{Slug}'", slug);

        var articles = await _unitOfWork.Repository<Article>()
            .FindAsync(a => a.Slug == slug && !a.IsDeleted, ct);

        var article = articles.FirstOrDefault();
        if (article == null)
        {
            _logger.LogWarning("Article with slug '{Slug}' was not found", slug);
            return Result<ArticleDto>.NotFound($"Article with slug '{slug}' was not found.");
        }

        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article));
    }

    public async Task<Result<ArticleDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var article = await _unitOfWork.Repository<Article>().GetByIdAsync(id, ct);
        if (article == null || article.IsDeleted)
        {
            _logger.LogWarning("Article with Id '{ArticleId}' was not found", id);
            return Result<ArticleDto>.NotFound($"Article with ID '{id}' was not found.");
        }

        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article));
    }

    public async Task<Result<ArticleDto>> CreateAsync(CreateArticleDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<ArticleDto>.Failure("Article title is required.");

        if (string.IsNullOrWhiteSpace(dto.Slug))
            dto.Slug = dto.Title.Trim().Replace(" ", "-").ToLower();

        var existing = await _unitOfWork.Repository<Article>()
            .FindAsync(a => a.Slug == dto.Slug && !a.IsDeleted, ct);

        if (existing.Any())
            return Result<ArticleDto>.Failure($"An article with slug '{dto.Slug}' already exists.");

        var article = _mapper.Map<Article>(dto);
        article.AuthorName = string.IsNullOrWhiteSpace(dto.AuthorName) ? "دکتر محمد حسین شفیع‌آبادی" : dto.AuthorName;

        await _unitOfWork.Repository<Article>().AddAsync(article, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Article '{ArticleTitle}' created with ID {ArticleId}", article.Title, article.Id);
        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article), 201);
    }

    public async Task<Result<ArticleDto>> UpdateAsync(Guid id, UpdateArticleDto dto, CancellationToken ct = default)
    {
        var article = await _unitOfWork.Repository<Article>().GetByIdAsync(id, ct);
        if (article == null || article.IsDeleted)
            return Result<ArticleDto>.NotFound($"Article with ID '{id}' was not found.");

        _mapper.Map(dto, article);
        article.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Article>().Update(article);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Article '{ArticleTitle}' (ID {ArticleId}) updated successfully", article.Title, article.Id);
        return Result<ArticleDto>.Success(_mapper.Map<ArticleDto>(article));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var article = await _unitOfWork.Repository<Article>().GetByIdAsync(id, ct);
        if (article == null || article.IsDeleted)
            return Result.NotFound($"Article with ID '{id}' was not found.");

        _unitOfWork.Repository<Article>().SoftDelete(article);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Article with ID {ArticleId} soft-deleted successfully", id);
        return Result.Success();
    }
}
