using FluentAssertions;
using ShafieiAbadi.Application.Common;

namespace ShafieiAbadi.UnitTests.ApplicationTests;

public class ResultPatternTests
{
    [Fact]
    public void SuccessResult_ShouldHaveSuccessTrueAnd200Status()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void FailureResult_ShouldHaveSuccessFalseAndCustomStatusCode()
    {
        // Act
        var result = Result.Failure("خطای آزمایشی", 400);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorMessage.Should().Be("خطای آزمایشی");
    }

    [Fact]
    public void GenericResult_ShouldContainDataPayload()
    {
        // Act
        var result = Result<string>.Success("دکتر محمد حسین شفیع‌آبادی");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("دکتر محمد حسین شفیع‌آبادی");
    }

    [Fact]
    public void PagedResult_ShouldCalculateTotalPagesAndNavigationFlags()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2", "Item 3" };
        var pagedResult = new PagedResult<string>(items, 25, pageNumber: 2, pageSize: 10);

        // Assert
        pagedResult.TotalCount.Should().Be(25);
        pagedResult.PageNumber.Should().Be(2);
        pagedResult.PageSize.Should().Be(10);
        pagedResult.TotalPages.Should().Be(3);
        pagedResult.HasPreviousPage.Should().BeTrue();
        pagedResult.HasNextPage.Should().BeTrue();
    }
}
