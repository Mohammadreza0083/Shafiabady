using FluentAssertions;
using ShafieiAbadi.Infrastructure.Authentication;

namespace ShafieiAbadi.UnitTests.InfrastructureTests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnValidPbkdf2Hash()
    {
        // Arrange
        var password = "SuperSecretPassword123!#";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("pbkdf2$100000$");
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "DrShafiei@Secure2026!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var password = "DrShafiei@Secure2026!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword("WrongPassword123!", hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldSupportLegacySha256Hash()
    {
        // Arrange: SHA-256 base64 hash of "Admin123!"
        // SHA-256("Admin123!") = jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=
        var password = "Admin123!";
        var sha256Bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password));
        var legacyHash = Convert.ToBase64String(sha256Bytes);

        // Act
        var isValid = _hasher.VerifyPassword(password, legacyHash);

        // Assert
        isValid.Should().BeTrue();
    }
}
