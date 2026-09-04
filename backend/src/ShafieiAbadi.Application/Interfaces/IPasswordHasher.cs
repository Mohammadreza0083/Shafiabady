namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Provides secure cryptographic hashing and verification for sensitive administrator credentials.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the raw password using a cryptographic algorithm with a random salt.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies whether a raw password matches an existing stored hash (with backward compatibility for legacy SHA-256 and plaintext seeds).
    /// </summary>
    bool VerifyPassword(string password, string storedHash);
}
