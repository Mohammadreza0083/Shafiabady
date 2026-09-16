using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;

namespace ShafieiAbadi.Api.Controllers;

/// <summary>
/// Provides secure administrator authentication, HttpOnly session cookie issuance, and identity verification.
/// </summary>
public class AuthController : BaseApiController
{
    private const string AuthCookieName = "AdminAuthToken";
    private const string RefreshCookieName = "AdminRefreshToken";
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates an administrator, issues secure HttpOnly JWT (5 min) and Refresh Token (1 or 30 days) cookies.
    /// Protected against brute-force attacks via fixed-window rate limiting.
    /// </summary>
    /// <param name="request">Administrator credentials and RememberMe preference.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result containing administrator profile metadata.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("auth-rate-limit")]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleResult(Result<AdminUserDto>.Unauthorized(result.ErrorMessage ?? "Invalid credentials."));
        }

        // 1. Append JWT strictly to HttpOnly, Secure, and SameSite=Strict cookie with 5-minute expiration
        Response.Cookies.Append(AuthCookieName, result.Data.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = result.Data.ExpiresAt,
            Path = "/"
        });

        // 2. Append Refresh Token to separate HttpOnly cookie with 1-day (24h) or 30-day expiration
        Response.Cookies.Append(RefreshCookieName, result.Data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = result.Data.RefreshTokenExpiresAt,
            Path = "/"
        });

        // Return user profile data without exposing raw JWT in JSON response body
        return Ok(Result<AdminUserDto>.Success(result.Data.User));
    }

    /// <summary>
    /// Logs out the administrator by revoking stored refresh tokens and deleting authentication cookies.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized success result.</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(username))
        {
            await _authService.LogoutAsync(username, ct);
        }

        var deleteOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        Response.Cookies.Delete(AuthCookieName, deleteOptions);
        Response.Cookies.Delete(RefreshCookieName, deleteOptions);

        return Ok(Result.Success());
    }

    /// <summary>
    /// Verifies current administrator session and returns authenticated identity profile.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Administrator profile details if token is valid.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(username))
        {
            return HandleResult(Result<AdminUserDto>.Unauthorized("User identity claim not found."));
        }

        var result = await _authService.GetUserByUsernameAsync(username, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Refreshes administrator session using HttpOnly Refresh Token cookie, re-issuing a renewed 5-minute JWT cookie.
    /// </summary>
    /// <param name="requestDto">Optional request payload with token if cookies are unavailable.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Administrator profile details and refreshed cookies.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? requestDto, CancellationToken ct)
    {
        // 1. Read refresh token from HttpOnly cookie or request body
        var refreshToken = Request.Cookies[RefreshCookieName] ?? requestDto?.RefreshToken;

        Result<LoginResultDto> result;

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            result = await _authService.RefreshTokenByTokenAsync(refreshToken, ct);
        }
        else if (User.Identity?.IsAuthenticated == true)
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(username))
            {
                return HandleResult(Result<AdminUserDto>.Unauthorized("User identity claim not found."));
            }
            result = await _authService.RefreshTokenAsync(username, ct);
        }
        else
        {
            return HandleResult(Result<AdminUserDto>.Unauthorized("No active refresh token found. Please log in."));
        }

        if (!result.IsSuccess || result.Data == null)
        {
            // Clear stale cookies on failed refresh
            var deleteOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };
            Response.Cookies.Delete(AuthCookieName, deleteOptions);
            Response.Cookies.Delete(RefreshCookieName, deleteOptions);

            return HandleResult(Result<AdminUserDto>.Unauthorized(result.ErrorMessage ?? "Invalid token refresh request."));
        }

        // Re-issue Access Token cookie (5 min)
        Response.Cookies.Append(AuthCookieName, result.Data.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = result.Data.ExpiresAt,
            Path = "/"
        });

        // Re-issue rotated Refresh Token cookie
        Response.Cookies.Append(RefreshCookieName, result.Data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = result.Data.RefreshTokenExpiresAt,
            Path = "/"
        });

        return Ok(Result<AdminUserDto>.Success(result.Data.User));
    }

    /// <summary>
    /// Changes the password of the currently authenticated administrator and invalidates active refresh tokens.
    /// </summary>
    /// <param name="dto">Change password credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result.</returns>
    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("auth-rate-limit")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
    {
        var username = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return HandleResult(Result.Unauthorized("User identity claim not found."));
        }

        var result = await _authService.ChangePasswordAsync(username, dto, ct);
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return Ok(Result.Success());
    }
}
