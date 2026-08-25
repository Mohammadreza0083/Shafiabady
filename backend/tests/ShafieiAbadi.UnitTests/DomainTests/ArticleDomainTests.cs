using FluentAssertions;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.UnitTests.DomainTests;

public class ArticleDomainTests
{
    [Fact]
    public void Article_ShouldInitializeWithDefaultAuthorName()
    {
        // Act
        var article = new Article();

        // Assert
        article.AuthorName.Should().Be("دکتر محمد حسین شفیع‌آبادی");
        article.IsPublished.Should().BeTrue();
        article.ViewsCount.Should().Be(0);
        article.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Book_ShouldStoreExternalBuyAndDigitalDownloadLinks()
    {
        // Arrange & Act
        var book = new Book
        {
            Title = "معماری تمیز در دات‌نت ۹",
            ExternalBuyLink = "https://publisher.com/buy-clean-arch",
            DigitalDownloadLink = "https://dl.shafieiabadi.ir/books/clean-arch.pdf"
        };

        // Assert
        book.ExternalBuyLink.Should().Be("https://publisher.com/buy-clean-arch");
        book.DigitalDownloadLink.Should().Be("https://dl.shafieiabadi.ir/books/clean-arch.pdf");
    }

    [Fact]
    public void Course_ShouldStoreAparatVideoAndPricingDetails()
    {
        // Arrange & Act
        var course = new Course
        {
            Title = "دوره جامع معماری سیستم",
            AparatVideoUrl = "https://www.aparat.com/v/sample123",
            OriginalPrice = 5_000_000,
            DiscountedPrice = 3_500_000,
            DiscountPercentage = 30
        };

        // Assert
        course.AparatVideoUrl.Should().Be("https://www.aparat.com/v/sample123");
        course.OriginalPrice.Should().Be(5_000_000);
        course.DiscountedPrice.Should().Be(3_500_000);
        course.DiscountPercentage.Should().Be(30);
    }
}
