using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

public interface IArticleService
{
    Task<Result<PagedResult<ArticleDto>>> GetPagedArticlesAsync(PaginationParams paginationParams, CancellationToken ct = default);
    Task<Result<ArticleDto>> GetArticleBySlugAsync(string slug, CancellationToken ct = default);
    Task<Result<ArticleDto>> GetArticleByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ArticleDto>> CreateArticleAsync(CreateArticleDto dto, CancellationToken ct = default);
    Task<Result<ArticleDto>> UpdateArticleAsync(UpdateArticleDto dto, CancellationToken ct = default);
    Task<Result> DeleteArticleAsync(Guid id, CancellationToken ct = default);
}
