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
/// Internal result container holding the authenticated administrator profile and the generated JWT.
/// </summary>
public class LoginResultDto
{
    public AdminUserDto User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
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
