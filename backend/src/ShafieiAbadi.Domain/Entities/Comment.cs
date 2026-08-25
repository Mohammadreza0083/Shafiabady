using ShafieiAbadi.Domain.Common;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Domain.Entities;

/// <summary>
/// Represents a user review, comment, or nested reply attached to an article, book, or course.
/// </summary>
public class Comment : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Name of the guest user or student submitting the comment.
    /// </summary>
    public string GuestName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the commenter (used for gravatar or admin notification).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Plain-text content of the comment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Current approval status (Pending, Approved, Rejected).
    /// </summary>
    public CommentStatus Status { get; set; } = CommentStatus.Pending;

    /// <summary>
    /// Optional self-referencing foreign key for recursive nested replies.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Navigation property to parent comment.
    /// </summary>
    public Comment? ParentComment { get; set; }

    /// <summary>
    /// Navigation collection of nested replies (unlimited recursive depth).
    /// </summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    /// <summary>
    /// Optional foreign key referencing an associated Article.
    /// </summary>
    public Guid? ArticleId { get; set; }

    /// <summary>
    /// Navigation property to Article.
    /// </summary>
    public Article? Article { get; set; }

    /// <summary>
    /// Optional foreign key referencing an associated Book.
    /// </summary>
    public Guid? BookId { get; set; }

    /// <summary>
    /// Navigation property to Book.
    /// </summary>
    public Book? Book { get; set; }

    /// <summary>
    /// Optional foreign key referencing an associated Course.
    /// </summary>
    public Guid? CourseId { get; set; }

    /// <summary>
    /// Navigation property to Course.
    /// </summary>
    public Course? Course { get; set; }
}
