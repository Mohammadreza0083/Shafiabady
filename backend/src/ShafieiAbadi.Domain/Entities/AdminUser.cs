using ShafieiAbadi.Domain.Common;

namespace ShafieiAbadi.Domain.Entities;

/// <summary>
/// Represents an authenticated system administrator with administrative dashboard access.
/// </summary>
public class AdminUser : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Unique administrative login username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Administrative email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Cryptographic hash of the administrator password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the administrator.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the administrator account is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC timestamp of the most recent successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Secure cryptographic refresh token for extending admin sessions without re-authenticating.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// UTC expiration timestamp of the active refresh token.
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }
}
