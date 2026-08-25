using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CommentService> _logger;

    public CommentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CommentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<CommentDto>>> GetApprovedCommentsTreeAsync(Guid? articleId, Guid? bookId, Guid? courseId, CancellationToken ct = default)
    {
        Expression<Func<Comment, bool>> predicate = c =>
            !c.IsDeleted &&
            c.Status == CommentStatus.Approved &&
            (!articleId.HasValue || c.ArticleId == articleId.Value) &&
            (!bookId.HasValue || c.BookId == bookId.Value) &&
            (!courseId.HasValue || c.CourseId == courseId.Value);

        var comments = await _unitOfWork.Repository<Comment>().FindAsync(predicate, ct);
        var dtos = _mapper.Map<List<CommentDto>>(comments);

        // Build infinite nested tree in memory
        var lookup = dtos.ToDictionary(c => c.Id);
        var rootComments = new List<CommentDto>();

        foreach (var comment in dtos)
        {
            if (comment.ParentCommentId.HasValue && lookup.TryGetValue(comment.ParentCommentId.Value, out var parent))
            {
                parent.Replies.Add(comment);
            }
            else
            {
                rootComments.Add(comment);
            }
        }

        return Result<List<CommentDto>>.Success(rootComments);
    }

    public async Task<Result<PagedResult<CommentDto>>> GetAllCommentsPagedAsync(int pageNumber, int pageSize, CommentStatus? statusFilter = null, CancellationToken ct = default)
    {
        Expression<Func<Comment, bool>> predicate = c =>
            !c.IsDeleted &&
            (!statusFilter.HasValue || c.Status == statusFilter.Value);

        Func<IQueryable<Comment>, IOrderedQueryable<Comment>> orderBy = q => q.OrderByDescending(c => c.CreatedAt);

        var (items, totalCount) = await _unitOfWork.Repository<Comment>()
            .GetPagedAsync(pageNumber, pageSize, predicate, orderBy, "", ct);

        var dtos = _mapper.Map<IReadOnlyList<CommentDto>>(items);
        return Result<PagedResult<CommentDto>>.Success(new PagedResult<CommentDto>(dtos, totalCount, pageNumber, pageSize));
    }

    public async Task<Result<CommentDto>> CreateCommentAsync(CreateCommentDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.GuestName) || string.IsNullOrWhiteSpace(dto.Text))
            return Result<CommentDto>.Failure("نام و متن دیدگاه الزامی هستند.");

        var comment = _mapper.Map<Comment>(dto);
        comment.Status = CommentStatus.Pending; // All guest comments require moderation

        await _unitOfWork.Repository<Comment>().AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("New comment submitted by {GuestName} for moderation (CommentId: {CommentId})", comment.GuestName, comment.Id);
        return Result<CommentDto>.Success(_mapper.Map<CommentDto>(comment), 201);
    }

    public async Task<Result> UpdateCommentStatusAsync(Guid commentId, CommentStatus status, CancellationToken ct = default)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(commentId, ct);
        if (comment == null || comment.IsDeleted)
            return Result.NotFound("دیدگاه مورد نظر یافت نشد.");

        comment.Status = status;
        comment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Comment>().Update(comment);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Comment {CommentId} status updated to {Status}", commentId, status);
        return Result.Success();
    }

    public async Task<Result> DeleteCommentAsync(Guid commentId, CancellationToken ct = default)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(commentId, ct);
        if (comment == null || comment.IsDeleted)
            return Result.NotFound("دیدگاه مورد نظر یافت نشد.");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Comment>().Update(comment);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Comment {CommentId} soft-deleted successfully", commentId);
        return Result.Success();
    }
}
