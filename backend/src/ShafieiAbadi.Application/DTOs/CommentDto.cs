using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.DTOs;

/// <summary>
/// Data transfer object representing a comment and its nested replies tree.
/// </summary>
public class CommentDto
{
    public Guid Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public CommentStatus Status { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid? ArticleId { get; set; }
    public Guid? BookId { get; set; }
    public Guid? CourseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

/// <summary>
/// Request payload for submitting a new public comment or reply.
/// </summary>
public class CreateCommentDto
{
    public string GuestName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public Guid? ArticleId { get; set; }
    public Guid? BookId { get; set; }
    public Guid? CourseId { get; set; }
}

/// <summary>
/// Request payload for moderating a comment status (Approve or Reject).
/// </summary>
public class UpdateCommentStatusDto
{
    public Guid CommentId { get; set; }
    public CommentStatus Status { get; set; }
}
