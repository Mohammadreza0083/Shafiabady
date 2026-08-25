using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

public interface IBookService
{
    Task<Result<PagedResult<BookDto>>> GetPagedBooksAsync(PaginationParams paginationParams, CancellationToken ct = default);
    Task<Result<BookDto>> GetBookBySlugAsync(string slug, CancellationToken ct = default);
    Task<Result<BookDto>> GetBookByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<BookDto>> CreateBookAsync(CreateBookDto dto, CancellationToken ct = default);
    Task<Result<BookDto>> UpdateBookAsync(UpdateBookDto dto, CancellationToken ct = default);
    Task<Result> DeleteBookAsync(Guid id, CancellationToken ct = default);
}
