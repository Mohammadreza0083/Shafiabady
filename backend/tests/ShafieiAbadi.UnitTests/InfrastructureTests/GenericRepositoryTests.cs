using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Infrastructure.Data;
using ShafieiAbadi.Infrastructure.Repositories;

namespace ShafieiAbadi.UnitTests.InfrastructureTests;

public class GenericRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly GenericRepository<Article> _repository;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new GenericRepository<Article>(_context);
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldPersistAndRetrieveEntity()
    {
        // Arrange
        var article = new Article
        {
            Title = "C# Testing Guide",
            Slug = "csharp-testing-guide",
            Summary = "Comprehensive guide for testing in .NET",
            Content = "<p>Content</p>",
            ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97",
            CategoryId = Guid.NewGuid()
        };

        // Act
        await _repository.AddAsync(article);
        await _context.SaveChangesAsync();
        var retrieved = await _repository.GetByIdAsync(article.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("C# Testing Guide");
        retrieved.AuthorName.Should().Be("دکتر محمد حسین شفیع‌آبادی");
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPaginationSlice()
    {
        // Arrange
        var catId = Guid.NewGuid();
        for (int i = 1; i <= 15; i++)
        {
            await _repository.AddAsync(new Article
            {
                Title = $"Article #{i}",
                Slug = $"article-{i}",
                Summary = $"Summary {i}",
                Content = $"<p>Content {i}</p>",
                ThumbnailUrl = "/placeholder.jpg",
                CategoryId = catId
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(pageNumber: 2, pageSize: 5);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }
}
