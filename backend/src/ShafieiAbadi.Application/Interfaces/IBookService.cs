using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Service contract defining operations for managing and retrieving academic books and publications.
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Retrieves a paginated list of books filtered by search criteria.
    /// </summary>
    Task<Result<PagedResult<BookDto>>> GetPagedAsync(PaginationParams @params, CancellationToken ct = default);

    /// <summary>
    /// Retrieves full book details, purchase hyperlinks, and download links by URL slug.
    /// </summary>
    Task<Result<BookDto>> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Retrieves book details by unique identifier.
    /// </summary>
    Task<Result<BookDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new book entry in the catalog.
    /// </summary>
    Task<Result<BookDto>> CreateAsync(CreateBookDto dto, CancellationToken ct = default);

    /// <summary>
    /// Updates book metadata, ISBN, and external links.
    /// </summary>
    Task<Result<BookDto>> UpdateAsync(Guid id, UpdateBookDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a book by its unique identifier.
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
