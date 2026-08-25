using FluentAssertions;
using ShafieiAbadi.Domain.Entities;
using ShafieiAbadi.Domain.Enums;

namespace ShafieiAbadi.UnitTests.DomainTests;

public class ViewLogDomainTests
{
    [Fact]
    public void IsWithin24Hours_WhenViewOccurred1HourAgo_ShouldReturnTrue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var viewLog = new ViewLog
        {
            IpAddress = "192.168.1.100",
            EntityType = EntityType.Article,
            EntityId = Guid.NewGuid(),
            ViewedAt = now.AddHours(-1)
        };

        // Act
        var isWithin24Hours = viewLog.IsWithin24Hours(now);

        // Assert
        isWithin24Hours.Should().BeTrue();
    }

    [Fact]
    public void IsWithin24Hours_WhenViewOccurred25HoursAgo_ShouldReturnFalse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var viewLog = new ViewLog
        {
            IpAddress = "192.168.1.100",
            EntityType = EntityType.Article,
            EntityId = Guid.NewGuid(),
            ViewedAt = now.AddHours(-25)
        };

        // Act
        var isWithin24Hours = viewLog.IsWithin24Hours(now);

        // Assert
        isWithin24Hours.Should().BeFalse();
    }
}
