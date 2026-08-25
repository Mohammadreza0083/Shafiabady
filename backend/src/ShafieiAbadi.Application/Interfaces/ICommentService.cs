using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Service contract defining operations for submitting, moderating, and recursively tree-organizing comments.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Retrieves a hierarchical tree of approved comments and nested replies for an entity.
    /// </summary>
    Task<Result<List<CommentDto>>> GetApprovedCommentsTreeAsync(EntityType type, Guid entityId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a paginated list of comments for the administrative moderation queue.
    /// </summary>
    Task<Result<PagedResult<CommentDto>>> GetPagedCommentsForAdminAsync(int pageNumber, int pageSize, CommentStatus? statusFilter = null, CancellationToken ct = default);

    /// <summary>
    /// Submits a new user comment or reply with initial Pending status.
    /// </summary>
    Task<Result<CommentDto>> CreateCommentAsync(CreateCommentDto dto, CancellationToken ct = default);

    /// <summary>
    /// Updates a comment's moderation status (Approved or Rejected).
    /// </summary>
    Task<Result> UpdateCommentStatusAsync(UpdateCommentStatusDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a comment by its unique identifier.
    /// </summary>
    Task<Result> DeleteCommentAsync(Guid id, CancellationToken ct = default);
}
