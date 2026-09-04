using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Application.Services;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.UnitTests.ApplicationTests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IGenericRepository<AdminUser>> _mockUserRepo;
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockUserRepo = new Mock<IGenericRepository<AdminUser>>();
        _mockJwtGenerator = new Mock<IJwtTokenGenerator>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _mockUow.Setup(u => u.Repository<AdminUser>()).Returns(_mockUserRepo.Object);

        _service = new AuthService(
            _mockUow.Object,
            _mockJwtGenerator.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_WithRememberMeFalse_ShouldSetRefreshTokenExpiryTo1Day()
    {
        // Arrange
        var user = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AdminUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminUser> { user });

        _mockPasswordHasher.Setup(h => h.VerifyPassword("Pass123!", "hashedPassword"))
            .Returns(true);

        _mockJwtGenerator.Setup(j => j.GenerateToken(user))
            .Returns(("jwt-token", DateTime.UtcNow.AddMinutes(5)));

        var request = new LoginRequestDto
        {
            UsernameOrEmail = "admin",
            Password = "Pass123!",
            RememberMe = false
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Data.RefreshTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromMinutes(5));
        user.RefreshToken.Should().Be(result.Data.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WithRememberMeTrue_ShouldSetRefreshTokenExpiryTo30Days()
    {
        // Arrange
        var user = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AdminUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminUser> { user });

        _mockPasswordHasher.Setup(h => h.VerifyPassword("Pass123!", "hashedPassword"))
            .Returns(true);

        _mockJwtGenerator.Setup(j => j.GenerateToken(user))
            .Returns(("jwt-token", DateTime.UtcNow.AddMinutes(5)));

        var request = new LoginRequestDto
        {
            UsernameOrEmail = "admin",
            Password = "Pass123!",
            RememberMe = true
        };

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Data.RefreshTokenExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(5));
        user.RefreshToken.Should().Be(result.Data.RefreshToken);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidDetails_ShouldUpdateHashAndInvalidateRefreshToken()
    {
        // Arrange
        var user = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "oldHash",
            RefreshToken = "activeRefreshToken",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(10),
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AdminUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminUser> { user });

        _mockPasswordHasher.Setup(h => h.VerifyPassword("OldPassword123!", "oldHash"))
            .Returns(true);

        _mockPasswordHasher.Setup(h => h.HashPassword("NewPassword123!@#"))
            .Returns("newPbkdf2Hash");

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!@#",
            ConfirmNewPassword = "NewPassword123!@#"
        };

        // Act
        var result = await _service.ChangePasswordAsync("admin", dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("newPbkdf2Hash");
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiresAt.Should().BeNull();
        _mockUserRepo.Verify(r => r.Update(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "realHash",
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AdminUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminUser> { user });

        _mockPasswordHasher.Setup(h => h.VerifyPassword("WrongOldPass!", "realHash"))
            .Returns(false);

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongOldPass!",
            NewPassword = "NewPassword123!@#",
            ConfirmNewPassword = "NewPassword123!@#"
        };

        // Act
        var result = await _service.ChangePasswordAsync("admin", dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        user.PasswordHash.Should().Be("realHash");
    }

    [Fact]
    public async Task RefreshTokenByTokenAsync_WithValidToken_ShouldRotateToken()
    {
        // Arrange
        var user = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            RefreshToken = "existingToken",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(5),
            IsActive = true
        };

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AdminUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminUser> { user });

        _mockJwtGenerator.Setup(j => j.GenerateToken(user))
            .Returns(("new-jwt", DateTime.UtcNow.AddMinutes(5)));

        // Act
        var result = await _service.RefreshTokenByTokenAsync("existingToken");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.RefreshToken.Should().NotBe("existingToken"); // Rotated
        user.RefreshToken.Should().Be(result.Data.RefreshToken);
        _mockUserRepo.Verify(r => r.Update(user), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
