namespace ShafieiAbadi.Application.DTOs;

/// <summary>
/// Request credentials for administrator login.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Administrator username or email address.
    /// </summary>
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// Administrator plain-text password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether the session should be remembered for 30 days (default is false = 24 hours).
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Administrator profile details returned upon successful login or authentication check.
/// </summary>
public class AdminUserDto
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Internal result container holding the authenticated administrator profile, generated JWT, and refresh token.
/// </summary>
public class LoginResultDto
{
    public AdminUserDto User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}

/// <summary>
/// Request payload for renewing access tokens using an active refresh token.
/// </summary>
public class RefreshTokenRequestDto
{
    public string? RefreshToken { get; set; }
}

/// <summary>
/// Request payload for changing the administrative account password.
/// </summary>
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Current authentication session verification status.
/// </summary>
public class AuthStatusDto
{
    public bool IsAuthenticated { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}
