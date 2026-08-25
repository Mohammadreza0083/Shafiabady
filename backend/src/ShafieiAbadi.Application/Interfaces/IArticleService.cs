using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Service contract defining operations for managing and retrieving articles.
/// </summary>
public interface IArticleService
{
    /// <summary>
    /// Retrieves a paginated list of articles filtered by search keyword, category, or tag.
    /// </summary>
    Task<Result<PagedResult<ArticleDto>>> GetPagedAsync(PaginationParams @params, CancellationToken ct = default);

    /// <summary>
    /// Retrieves full article details by its unique URL slug.
    /// </summary>
    Task<Result<ArticleDto>> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Retrieves full article details by its unique identifier.
    /// </summary>
    Task<Result<ArticleDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates and publishes a new article.
    /// </summary>
    Task<Result<ArticleDto>> CreateAsync(CreateArticleDto dto, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing article's metadata and content.
    /// </summary>
    Task<Result<ArticleDto>> UpdateAsync(Guid id, UpdateArticleDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes an article by its unique identifier.
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
