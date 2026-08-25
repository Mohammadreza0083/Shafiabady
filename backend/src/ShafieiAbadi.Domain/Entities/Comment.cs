using ShafieiAbadi.Domain.Common;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.Domain.Entities;

public class Comment : BaseEntity, IAggregateRoot
{
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string Text { get; set; } = string.Empty;
    public CommentStatus Status { get; set; } = CommentStatus.Pending;

    // Self-referencing recursive hierarchy for infinite nested comments
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    // Entity associations (Polymorphic/Nullable FKs)
    public Guid? ArticleId { get; set; }
    public Article? Article { get; set; }

    public Guid? BookId { get; set; }
    public Book? Book { get; set; }

    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }
}
