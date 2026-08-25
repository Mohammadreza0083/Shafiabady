using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Endpoints for submitting public comments, querying recursive reply trees, and managing moderation statuses.
/// </summary>
public class CommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Retrieves a recursive tree of approved comments and nested replies for an entity.
    /// </summary>
    /// <param name="type">Target entity type (1: Article, 2: Book, 3: Course).</param>
    /// <param name="entityId">Unique target entity GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Hierarchical tree of approved root comments with nested replies.</returns>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(Result<List<CommentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommentsTree([FromQuery] EntityType type, [FromQuery] Guid entityId, CancellationToken ct)
    {
        var result = await _commentService.GetApprovedCommentsTreeAsync(type, entityId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves paginated comments for the administrative moderation queue (Admin only).
    /// </summary>
    /// <param name="pageNumber">1-based page number (defaults to 1).</param>
    /// <param name="pageSize">Number of items per page (defaults to 10).</param>
    /// <param name="status">Optional filter by status (0: Pending, 1: Approved, 2: Rejected).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated collection of comments for moderation.</returns>
    [HttpGet("admin/paged")]
    [Authorize]
    [ProducesResponseType(typeof(Result<PagedResult<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PagedResult<CommentDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPagedForAdmin([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] CommentStatus? status = null, CancellationToken ct = default)
    {
        var result = await _commentService.GetPagedCommentsForAdminAsync(pageNumber, pageSize, status, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Submits a new user comment or reply with initial Pending status.
    /// </summary>
    /// <param name="dto">Comment submission payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created comment details awaiting review.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CommentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto, CancellationToken ct)
    {
        var result = await _commentService.CreateCommentAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates a comment's moderation approval status (Admin only).
    /// </summary>
    /// <param name="dto">Status update payload (Approved / Rejected).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result indicating success or failure.</returns>
    [HttpPut("status")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateCommentStatusDto dto, CancellationToken ct)
    {
        var result = await _commentService.UpdateCommentStatusAsync(dto, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Soft-deletes a comment by its unique identifier (Admin only).
    /// </summary>
    /// <param name="id">Unique comment GUID to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result indicating success or failure.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _commentService.DeleteCommentAsync(id, ct);
        return HandleResult(result);
    }
}
