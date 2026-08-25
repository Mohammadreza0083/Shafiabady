using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Application.Mappings;
using ShafieiAbadi.Application.Services;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.UnitTests.ApplicationTests;

public class CommentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IGenericRepository<Comment>> _mockCommentRepo;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CommentService>> _mockLogger;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockCommentRepo = new Mock<IGenericRepository<Comment>>();
        _mockLogger = new Mock<ILogger<CommentService>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _mockUow.Setup(u => u.Repository<Comment>()).Returns(_mockCommentRepo.Object);

        _service = new CommentService(_mockUow.Object, _mapper, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateCommentAsync_ShouldSetStatusToPending()
    {
        // Arrange
        var dto = new CreateCommentDto
        {
            GuestName = "Ali Rezaei",
            Email = "ali@example.com",
            Text = "Great article, very comprehensive.",
            ArticleId = Guid.NewGuid()
        };

        // Act
        var result = await _service.CreateCommentAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(CommentStatus.Pending);
        _mockCommentRepo.Verify(r => r.AddAsync(It.Is<Comment>(c => c.Status == CommentStatus.Pending), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetApprovedCommentsTreeAsync_ShouldAssembleHierarchyCorrectly()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var rootId = Guid.NewGuid();
        var replyId = Guid.NewGuid();

        var comments = new List<Comment>
        {
            new() { Id = rootId, ArticleId = articleId, GuestName = "Student 1", Text = "Root Comment", Status = CommentStatus.Approved },
            new() { Id = replyId, ArticleId = articleId, ParentCommentId = rootId, GuestName = "Dr. Shafiei Abadi", Text = "Reply", Status = CommentStatus.Approved }
        };

        _mockCommentRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Comment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);

        // Act
        var result = await _service.GetApprovedCommentsTreeAsync(EntityType.Article, articleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Id.Should().Be(rootId);
        result.Data[0].Replies.Should().HaveCount(1);
        result.Data[0].Replies[0].Id.Should().Be(replyId);
    }
}
