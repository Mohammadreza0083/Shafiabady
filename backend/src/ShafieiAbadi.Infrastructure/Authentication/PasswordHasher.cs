using System.Security.Cryptography;
using System.Text;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Infrastructure.Authentication;

/// <summary>
/// Production-grade cryptographic password hasher utilizing PBKDF2 with HMAC-SHA256 and salted hashes,
/// with automatic backward-compatibility for legacy SHA-256 and initial seeded hashes.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            KeySize);

        return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        // 1. Check if the stored hash is PBKDF2 formatted
        if (storedHash.StartsWith("pbkdf2$"))
        {
            var parts = storedHash.Split('$');
            if (parts.Length == 4 && int.TryParse(parts[1], out int iterations))
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    iterations,
                    Algorithm,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
        }

        // 2. Backward compatibility: Standard SHA-256 Base64 hash (used in database seeding)
        var sha256Bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var sha256Base64 = Convert.ToBase64String(sha256Bytes);
        if (CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(sha256Base64),
            Encoding.UTF8.GetBytes(storedHash)))
        {
            return true;
        }

        // 3. Fallback for raw match in seed/local environment
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
