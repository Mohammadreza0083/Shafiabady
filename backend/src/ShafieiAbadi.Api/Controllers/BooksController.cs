using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Endpoints for managing, browsing, and querying published academic books, textbooks, and purchase/download links.
/// </summary>
public class BooksController : BaseApiController
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Retrieves a paginated list of books to support infinite scrolling and search filtering.
    /// </summary>
    /// <param name="paginationParams">Filtering, search, and pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated book collection with total counts and pagination metadata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<BookDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams, CancellationToken ct)
    {
        var result = await _bookService.GetPagedAsync(paginationParams, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves full book details, purchase hyperlinks, and digital download links by unique URL slug.
    /// </summary>
    /// <param name="slug">Unique URL slug identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed book data payload.</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _bookService.GetBySlugAsync(slug, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves full book details by its unique identifier (GUID).
    /// </summary>
    /// <param name="id">Unique book GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed book data payload.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _bookService.GetByIdAsync(id, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new book entry in the catalog (Admin only).
    /// </summary>
    /// <param name="dto">Book creation payload with buy and download links.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created book details.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto, CancellationToken ct)
    {
        var result = await _bookService.CreateAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an existing book's metadata, ISBN, and download/buy links (Admin only).
    /// </summary>
    /// <param name="id">Unique book GUID to update.</param>
    /// <param name="dto">Updated book payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated book details.</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<BookDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookDto dto, CancellationToken ct)
    {
        var result = await _bookService.UpdateAsync(id, dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Soft-deletes a book by its unique identifier (Admin only).
    /// </summary>
    /// <param name="id">Unique book GUID to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result indicating success or failure.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _bookService.DeleteAsync(id, ct);
        return HandleResult(result);
    }
}
