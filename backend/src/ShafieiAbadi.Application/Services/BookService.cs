using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

/// <summary>
/// Service implementation for managing academic books and publications.
/// </summary>
public class BookService : IBookService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BookService> _logger;

    public BookService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BookService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<BookDto>>> GetPagedAsync(PaginationParams @params, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching books page {PageNumber} with size {PageSize}", @params.PageNumber, @params.PageSize);

        Expression<Func<Book, bool>> filter = b =>
            !b.IsDeleted &&
            (string.IsNullOrWhiteSpace(@params.Search) || b.Title.Contains(@params.Search) || b.Description.Contains(@params.Search) || b.Publisher.Contains(@params.Search)) &&
            (!@params.CategoryId.HasValue || b.CategoryId == @params.CategoryId.Value) &&
            (string.IsNullOrWhiteSpace(@params.CategorySlug) || @params.CategorySlug == "all" || @params.CategorySlug == "همه" || (b.Category != null && (b.Category.Slug == @params.CategorySlug || b.Category.Title == @params.CategorySlug)));

        var pagedEntities = await _unitOfWork.Repository<Book>().GetPagedAsync(
            @params.PageNumber,
            @params.PageSize,
            filter: filter,
            orderBy: b => b.CreatedAt,
            ascending: false,
            ct: ct,
            includes: new Expression<Func<Book, object>>[] { b => b.Category! });

        var dtos = _mapper.Map<IReadOnlyList<BookDto>>(pagedEntities.Items);
        var result = new PagedResult<BookDto>(dtos, pagedEntities.TotalCount, pagedEntities.PageNumber, pagedEntities.PageSize);

        return Result<PagedResult<BookDto>>.Success(result);
    }

    public async Task<Result<BookDto>> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving book by slug '{Slug}'", slug);

        var books = await _unitOfWork.Repository<Book>()
            .FindAsync(b => b.Slug == slug && !b.IsDeleted, ct);

        var book = books.FirstOrDefault();
        if (book == null)
        {
            _logger.LogWarning("Book with slug '{Slug}' was not found", slug);
            return Result<BookDto>.NotFound($"Book with slug '{slug}' was not found.");
        }

        return Result<BookDto>.Success(_mapper.Map<BookDto>(book));
    }

    public async Task<Result<BookDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Repository<Book>().GetByIdAsync(id, ct);
        if (book == null || book.IsDeleted)
        {
            _logger.LogWarning("Book with ID '{BookId}' was not found", id);
            return Result<BookDto>.NotFound($"Book with ID '{id}' was not found.");
        }

        return Result<BookDto>.Success(_mapper.Map<BookDto>(book));
    }

    public async Task<Result<BookDto>> CreateAsync(CreateBookDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<BookDto>.Failure("Book title is required.");

        if (string.IsNullOrWhiteSpace(dto.Slug))
            dto.Slug = dto.Title.Trim().Replace(" ", "-").ToLower();

        var existing = await _unitOfWork.Repository<Book>()
            .FindAsync(b => b.Slug == dto.Slug && !b.IsDeleted, ct);

        if (existing.Any())
            return Result<BookDto>.Failure($"A book with slug '{dto.Slug}' already exists.");

        var book = _mapper.Map<Book>(dto);
        await _unitOfWork.Repository<Book>().AddAsync(book, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Book '{BookTitle}' created with ID {BookId}", book.Title, book.Id);
        return Result<BookDto>.Success(_mapper.Map<BookDto>(book), 201);
    }

    public async Task<Result<BookDto>> UpdateAsync(Guid id, UpdateBookDto dto, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Repository<Book>().GetByIdAsync(id, ct);
        if (book == null || book.IsDeleted)
            return Result<BookDto>.NotFound($"Book with ID '{id}' was not found.");

        _mapper.Map(dto, book);
        book.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Book>().Update(book);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Book '{BookTitle}' (ID {BookId}) updated successfully", book.Title, book.Id);
        return Result<BookDto>.Success(_mapper.Map<BookDto>(book));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Repository<Book>().GetByIdAsync(id, ct);
        if (book == null || book.IsDeleted)
            return Result.NotFound($"Book with ID '{id}' was not found.");

        _unitOfWork.Repository<Book>().SoftDelete(book);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Book with ID {BookId} soft-deleted successfully", id);
        return Result.Success();
    }
}
