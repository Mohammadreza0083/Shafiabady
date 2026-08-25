using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShafieiAbadi.Api.Controllers;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.UnitTests.ApiTests;

public class ArticlesControllerTests
{
    private readonly Mock<IArticleService> _mockArticleService;
    private readonly ArticlesController _controller;

    public ArticlesControllerTests()
    {
        _mockArticleService = new Mock<IArticleService>();
        _controller = new ArticlesController(_mockArticleService.Object);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnOkWithStandardizedPagedResult()
    {
        // Arrange
        var pagedData = new PagedResult<ArticleDto>(new List<ArticleDto>
        {
            new() { Title = "Test Article 1", Slug = "test-1" }
        }, totalCount: 1, pageNumber: 1, pageSize: 10);

        var expectedResult = Result<PagedResult<ArticleDto>>.Success(pagedData);

        _mockArticleService.Setup(s => s.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.GetPaged(new PaginationParams(), CancellationToken.None);

        // Assert
        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(200);

        var payload = objectResult.Value as Result<PagedResult<ArticleDto>>;
        payload.Should().NotBeNull();
        payload!.IsSuccess.Should().BeTrue();
        payload.Data!.Items.Should().HaveCount(1);
    }
}
