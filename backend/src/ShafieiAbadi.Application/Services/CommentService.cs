using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Services;

/// <summary>
/// Service implementation for submitting, moderating, and organizing hierarchical comment trees.
/// </summary>
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

    public async Task<Result<List<CommentDto>>> GetApprovedCommentsTreeAsync(EntityType type, Guid entityId, CancellationToken ct = default)
    {
        _logger.LogInformation("Building approved comments tree for entity {EntityType} with ID {EntityId}", type, entityId);

        Expression<Func<Comment, bool>> filter = c =>
            !c.IsDeleted &&
            c.Status == CommentStatus.Approved &&
            ((type == EntityType.Article && c.ArticleId == entityId) ||
             (type == EntityType.Book && c.BookId == entityId) ||
             (type == EntityType.Course && c.CourseId == entityId));

        var allApproved = await _unitOfWork.Repository<Comment>().FindAsync(filter, ct);
        var dtos = _mapper.Map<List<CommentDto>>(allApproved);

        // Build recursive tree in memory
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

        return Result<List<CommentDto>>.Success(rootComments.OrderByDescending(c => c.CreatedAt).ToList());
    }

    public async Task<Result<PagedResult<CommentDto>>> GetPagedCommentsForAdminAsync(int pageNumber, int pageSize, CommentStatus? statusFilter = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving comments for admin moderation queue. Status filter: {StatusFilter}", statusFilter);

        Expression<Func<Comment, bool>> filter = c =>
            !c.IsDeleted &&
            (!statusFilter.HasValue || c.Status == statusFilter.Value);

        var pagedEntities = await _unitOfWork.Repository<Comment>().GetPagedAsync(
            pageNumber,
            pageSize,
            filter: filter,
            orderBy: c => c.CreatedAt,
            ascending: false,
            ct: ct);

        var dtos = _mapper.Map<IReadOnlyList<CommentDto>>(pagedEntities.Items);
        var result = new PagedResult<CommentDto>(dtos, pagedEntities.TotalCount, pagedEntities.PageNumber, pagedEntities.PageSize);

        return Result<PagedResult<CommentDto>>.Success(result);
    }

    public async Task<Result<CommentDto>> CreateCommentAsync(CreateCommentDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.GuestName) || string.IsNullOrWhiteSpace(dto.Text))
        {
            return Result<CommentDto>.Failure("Name and comment text are required.");
        }

        var comment = _mapper.Map<Comment>(dto);
        comment.Status = CommentStatus.Pending;

        await _unitOfWork.Repository<Comment>().AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("New comment submitted by '{GuestName}' for entity (Article: {ArticleId}, Book: {BookId}, Course: {CourseId}) with initial status Pending",
            comment.GuestName, comment.ArticleId, comment.BookId, comment.CourseId);

        return Result<CommentDto>.Success(_mapper.Map<CommentDto>(comment), 201);
    }

    public async Task<Result> UpdateCommentStatusAsync(UpdateCommentStatusDto dto, CancellationToken ct = default)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(dto.CommentId, ct);
        if (comment == null || comment.IsDeleted)
        {
            _logger.LogWarning("Comment with ID {CommentId} was not found for moderation update", dto.CommentId);
            return Result.NotFound($"Comment with ID '{dto.CommentId}' was not found.");
        }

        var oldStatus = comment.Status;
        comment.Status = dto.Status;
        comment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Comment>().Update(comment);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Comment {CommentId} status changed from {OldStatus} to {NewStatus}", comment.Id, oldStatus, comment.Status);
        return Result.Success();
    }

    public async Task<Result> DeleteCommentAsync(Guid id, CancellationToken ct = default)
    {
        var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(id, ct);
        if (comment == null || comment.IsDeleted)
        {
            return Result.NotFound($"Comment with ID '{id}' was not found.");
        }

        _unitOfWork.Repository<Comment>().SoftDelete(comment);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Comment with ID {CommentId} soft-deleted successfully", id);
        return Result.Success();
    }
}
