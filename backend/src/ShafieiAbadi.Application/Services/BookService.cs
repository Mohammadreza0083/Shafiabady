using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

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

    public async Task<Result<PagedResult<BookDto>>> GetPagedBooksAsync(PaginationParams p, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving books page {PageNumber} with size {PageSize}", p.PageNumber, p.PageSize);

        Expression<Func<Book, bool>> predicate = b =>
            !b.IsDeleted &&
            (!p.CategoryId.HasValue || b.CategoryId == p.CategoryId.Value) &&
            (string.IsNullOrWhiteSpace(p.SearchTerm) || b.Title.Contains(p.SearchTerm) || b.Description.Contains(p.SearchTerm) || b.Publisher.Contains(p.SearchTerm));

        Func<IQueryable<Book>, IOrderedQueryable<Book>> orderBy = q => q.OrderByDescending(b => b.CreatedAt);

        var (items, totalCount) = await _unitOfWork.Repository<Book>()
            .GetPagedAsync(p.PageNumber, p.PageSize, predicate, orderBy, "Category,Comments", ct);

        var dtos = _mapper.Map<IReadOnlyList<BookDto>>(items);
        var pagedResult = new PagedResult<BookDto>(dtos, totalCount, p.PageNumber, p.PageSize);

        return Result<PagedResult<BookDto>>.Success(pagedResult);
    }

    public async Task<Result<BookDto>> GetBookBySlugAsync(string slug, CancellationToken ct = default)
    {
        var books = await _unitOfWork.Repository<Book>()
            .FindAsync(b => b.Slug == slug && !b.IsDeleted, ct);

        var book = books.FirstOrDefault();
        if (book == null)
        {
            _logger.LogWarning("Book with slug '{Slug}' was not found", slug);
            return Result<BookDto>.NotFound($"کتاب با نامک '{slug}' یافت نشد.");
        }

        return Result<BookDto>.Success(_mapper.Map<BookDto>(book));
    }

    public async Task<Result<BookDto>> GetBookByIdAsync(Guid id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Repository<Book>().GetByIdAsync(id, ct);
        if (book == null || book.IsDeleted)
        {
            _logger.LogWarning("Book with Id '{Id}' was not found", id);
            return Result<BookDto>.NotFound("کتاب مورد نظر یافت نشد.");
        }

        return Result<BookDto>.Success(_mapper.Map<BookDto>(book));
    }

    public async Task<Result<BookDto>> CreateBookAsync(CreateBookDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<BookDto>.Failure("عنوان کتاب الزامی است.");

        var exists = await _unitOfWork.Repository<Book>()
            .ExistsAsync(b => b.Slug == dto.Slug && !b.IsDeleted, ct);
        if (exists)
            return Result<BookDto>.Failure("نامک (Slug) وارد شده تکراری است.");

        var book = _mapper.Map<Book>(dto);
        await _unitOfWork.Repository<Book>().AddAsync(book, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Book '{Title}' created successfully with Id {BookId}", book.Title, book.Id);
        return Result<BookDto>.Success(_mapper.Map<BookDto>(book), 201);
    }

    public async Task<Result<BookDto>> UpdateBookAsync(UpdateBookDto dto, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Repository<Book>().GetByIdAsync(dto.Id, ct);
        if (book == null || book.IsDeleted)
            return Result<BookDto>.NotFound("کتاب جهت ویرایش یافت نشد.");

        _mapper.Map(dto, book);
        book.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Book>().Update(book);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Book {BookId} updated successfully", book.Id);
        return Result<BookDto>.Success(_mapper.Map<BookDto>(book));
    }

    public async Task<Result> DeleteBookAsync(Guid id, CancellationToken ct = default)
    {
        var book = await _unitOfWork.Repository<Book>().GetByIdAsync(id, ct);
        if (book == null || book.IsDeleted)
            return Result.NotFound("کتاب جهت حذف یافت نشد.");

        book.IsDeleted = true;
        book.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Book>().Update(book);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Book {BookId} soft-deleted successfully", id);
        return Result.Success();
    }
}
