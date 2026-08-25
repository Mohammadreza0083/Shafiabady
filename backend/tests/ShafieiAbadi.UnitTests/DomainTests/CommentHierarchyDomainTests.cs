using FluentAssertions;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.UnitTests.DomainTests;

public class CommentHierarchyDomainTests
{
    [Fact]
    public void Comment_ShouldSupportInfiniteRecursiveNesting()
    {
        // Level 1: Root Comment
        var rootComment = new Comment
        {
            Id = Guid.NewGuid(),
            GuestName = "دانشجو ۱",
            Text = "مقاله بسیار عالی و مفیدی بود.",
            Status = CommentStatus.Approved
        };

        // Level 2: First Reply
        var replyLevel1 = new Comment
        {
            Id = Guid.NewGuid(),
            GuestName = "دکتر شفیع‌آبادی",
            Text = "با تشکر، موفق و پیروز باشید.",
            ParentCommentId = rootComment.Id,
            ParentComment = rootComment,
            Status = CommentStatus.Approved
        };
        rootComment.Replies.Add(replyLevel1);

        // Level 3: Nested Sub-reply
        var replyLevel2 = new Comment
        {
            Id = Guid.NewGuid(),
            GuestName = "دانشجو ۲",
            Text = "من هم در پیاده‌سازی DDD از این نکات استفاده کردم.",
            ParentCommentId = replyLevel1.Id,
            ParentComment = replyLevel1,
            Status = CommentStatus.Approved
        };
        replyLevel1.Replies.Add(replyLevel2);

        // Assert
        rootComment.Replies.Should().HaveCount(1);
        rootComment.Replies.First().Replies.Should().HaveCount(1);
        rootComment.Replies.First().Replies.First().GuestName.Should().Be("دانشجو ۲");
        rootComment.Replies.First().Replies.First().ParentCommentId.Should().Be(replyLevel1.Id);
    }
}
