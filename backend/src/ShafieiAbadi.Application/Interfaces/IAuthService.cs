using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Service contract defining operations for administrator authentication and session validation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates an administrator against stored credentials and returns the user details alongside a signed 5-minute JWT.
    /// </summary>
    /// <param name="request">Administrator login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result containing the authenticated profile and raw JWT token.</returns>
    Task<Result<LoginResultDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Retrieves administrator profile details by username.
    /// </summary>
    /// <param name="username">Administrator username.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Administrator profile DTO.</returns>
    Task<Result<AdminUserDto>> GetUserByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Refreshes administrator session and generates a fresh JWT token for an active user.
    /// </summary>
    /// <param name="username">Administrator username.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result containing updated administrator profile and fresh JWT token.</returns>
    Task<Result<LoginResultDto>> RefreshTokenAsync(string username, CancellationToken ct = default);
}
