namespace ShafieiAbadi.Domain.Enums;

/// <summary>
/// Moderation workflow status for user-submitted comments.
/// </summary>
public enum CommentStatus
{
    /// <summary>
    /// Awaiting admin moderation review.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Approved by administrator and publicly visible.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Rejected or flagged by administrator.
    /// </summary>
    Rejected = 2
}
