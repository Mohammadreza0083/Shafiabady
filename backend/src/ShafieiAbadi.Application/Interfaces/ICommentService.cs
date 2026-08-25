using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.Interfaces;

public interface ICommentService
{
    Task<Result<List<CommentDto>>> GetApprovedCommentsTreeAsync(Guid? articleId, Guid? bookId, Guid? courseId, CancellationToken ct = default);
    Task<Result<PagedResult<CommentDto>>> GetAllCommentsPagedAsync(int pageNumber, int pageSize, CommentStatus? statusFilter = null, CancellationToken ct = default);
    Task<Result<CommentDto>> CreateCommentAsync(CreateCommentDto dto, CancellationToken ct = default);
    Task<Result> UpdateCommentStatusAsync(Guid commentId, CommentStatus status, CancellationToken ct = default);
    Task<Result> DeleteCommentAsync(Guid commentId, CancellationToken ct = default);
}
