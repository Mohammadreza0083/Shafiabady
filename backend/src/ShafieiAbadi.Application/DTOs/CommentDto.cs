using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Application.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string Text { get; set; } = string.Empty;
    public CommentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid? ArticleId { get; set; }
    public Guid? BookId { get; set; }
    public Guid? CourseId { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

public class CreateCommentDto
{
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public Guid? ArticleId { get; set; }
    public Guid? BookId { get; set; }
    public Guid? CourseId { get; set; }
}

public class UpdateCommentStatusDto
{
    public Guid CommentId { get; set; }
    public CommentStatus Status { get; set; }
}
