using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates an administrator, issues a secure 5-minute HttpOnly JWT cookie, and returns user profile data.
    /// </summary>
    /// <param name="request">Administrator username/email and password credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Standardized result containing administrator profile metadata (JWT is secured in HttpOnly cookie).</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleResult(Result<AdminUserDto>.Unauthorized(result.ErrorMessage ?? "Invalid credentials."));
        }

        // Append JWT strictly to HttpOnly, Secure, and SameSite=Strict cookie with 5-minute expiration
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = result.Data.ExpiresAt,
            Path = "/"
        };

        Response.Cookies.Append(AuthCookieName, result.Data.Token, cookieOptions);

        // Return user profile data without exposing raw JWT in JSON response body
        return Ok(Result<AdminUserDto>.Success(result.Data.User));
    }

    /// <summary>
    /// Logs out the administrator by revoking and deleting the authentication cookie.
    /// </summary>
    /// <returns>Standardized success result.</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

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
    /// Refreshes administrator session and re-issues a fresh 5-minute HttpOnly JWT cookie.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Administrator profile details and refreshed cookie.</returns>
    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AdminUserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(CancellationToken ct)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(username))
        {
            return HandleResult(Result<AdminUserDto>.Unauthorized("User identity claim not found."));
        }

        var result = await _authService.RefreshTokenAsync(username, ct);
        if (!result.IsSuccess || result.Data == null)
        {
            return HandleResult(Result<AdminUserDto>.Unauthorized(result.ErrorMessage ?? "Invalid token refresh request."));
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = result.Data.ExpiresAt,
            Path = "/"
        };

        Response.Cookies.Append(AuthCookieName, result.Data.Token, cookieOptions);

        return Ok(Result<AdminUserDto>.Success(result.Data.User));
    }
}
