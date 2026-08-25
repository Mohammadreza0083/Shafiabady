using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Application.Services;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.UnitTests.ApplicationTests;

public class ViewTrackerServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IGenericRepository<ViewLog>> _mockViewRepo;
    private readonly Mock<IGenericRepository<Article>> _mockArticleRepo;
    private readonly Mock<ILogger<ViewTrackerService>> _mockLogger;
    private readonly ViewTrackerService _service;

    public ViewTrackerServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockViewRepo = new Mock<IGenericRepository<ViewLog>>();
        _mockArticleRepo = new Mock<IGenericRepository<Article>>();
        _mockLogger = new Mock<ILogger<ViewTrackerService>>();

        _mockUow.Setup(u => u.Repository<ViewLog>()).Returns(_mockViewRepo.Object);
        _mockUow.Setup(u => u.Repository<Article>()).Returns(_mockArticleRepo.Object);

        _service = new ViewTrackerService(_mockUow.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RecordViewAsync_WhenViewWithin24HoursExists_ShouldReturnFalseAndNotIncrementViews()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var existingLogs = new List<ViewLog>
        {
            new() { EntityId = articleId, EntityType = EntityType.Article, IpAddress = "127.0.0.1", ViewedAt = DateTime.UtcNow.AddMinutes(-30) }
        };

        _mockViewRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ViewLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLogs);

        // Act
        var result = await _service.RecordViewAsync(EntityType.Article, articleId, "127.0.0.1", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeFalse();
        _mockViewRepo.Verify(r => r.AddAsync(It.IsAny<ViewLog>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordViewAsync_WhenNoRecentViewExists_ShouldAddViewLogAndIncrementArticleCount()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var article = new Article { Id = articleId, Title = "Architecture Article", ViewsCount = 10 };

        _mockViewRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ViewLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ViewLog>());

        _mockArticleRepo.Setup(r => r.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        // Act
        var result = await _service.RecordViewAsync(EntityType.Article, articleId, "127.0.0.1", "Mozilla/5.0");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        article.ViewsCount.Should().Be(11);
        _mockViewRepo.Verify(r => r.AddAsync(It.IsAny<ViewLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
