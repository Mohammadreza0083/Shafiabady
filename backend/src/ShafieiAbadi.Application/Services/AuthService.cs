using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

/// <summary>
/// Service implementation for administrative authentication and session verification.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<LoginResultDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Admin login attempt with empty credentials");
            return Result<LoginResultDto>.Failure("Username and password are required.");
        }

        var users = await _unitOfWork.Repository<AdminUser>()
            .FindAsync(u => (u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail) && !u.IsDeleted && u.IsActive, ct);

        var user = users.FirstOrDefault();
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for non-existing or inactive user '{Username}'", request.UsernameOrEmail);
            return Result<LoginResultDto>.Unauthorized("Invalid username or password.");
        }

        // Verify password hash via IPasswordHasher
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt (invalid password) for user '{Username}'", request.UsernameOrEmail);
            return Result<LoginResultDto>.Unauthorized("Invalid username or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;

        // Generate Refresh Token: 30 days if RememberMe is true, else 1 day (24 hours)
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenExpiry = request.RememberMe
            ? DateTime.UtcNow.AddDays(30)
            : DateTime.UtcNow.AddDays(1);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiry;

        _unitOfWork.Repository<AdminUser>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Admin user '{Username}' logged in successfully (RememberMe: {RememberMe}, RefreshToken expires: {Expiry})",
            user.Username, request.RememberMe, refreshTokenExpiry);

        // Generate signed JWT with strict 5-minute lifespan
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        var userDto = new AdminUserDto
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            LastLoginAt = user.LastLoginAt
        };

        var response = new LoginResultDto
        {
            User = userDto,
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiry
        };

        return Result<LoginResultDto>.Success(response);
    }

    public async Task<Result<AdminUserDto>> GetUserByUsernameAsync(string username, CancellationToken ct = default)
    {
        var users = await _unitOfWork.Repository<AdminUser>()
            .FindAsync(u => u.Username == username && !u.IsDeleted && u.IsActive, ct);

        var user = users.FirstOrDefault();
        if (user == null)
        {
            return Result<AdminUserDto>.NotFound("User session not found.");
        }

        return Result<AdminUserDto>.Success(new AdminUserDto
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            LastLoginAt = user.LastLoginAt
        });
    }

    public async Task<Result<LoginResultDto>> RefreshTokenAsync(string username, CancellationToken ct = default)
    {
        var users = await _unitOfWork.Repository<AdminUser>()
            .FindAsync(u => u.Username == username && !u.IsDeleted && u.IsActive, ct);

        var user = users.FirstOrDefault();
        if (user == null)
        {
            _logger.LogWarning("Refresh token failed for non-existing or inactive user '{Username}'", username);
            return Result<LoginResultDto>.Unauthorized("User session not found or user is inactive.");
        }

        // Generate a fresh signed JWT with 5-minute lifespan
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        var userDto = new AdminUserDto
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            LastLoginAt = user.LastLoginAt
        };

        var response = new LoginResultDto
        {
            User = userDto,
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = user.RefreshToken ?? string.Empty,
            RefreshTokenExpiresAt = user.RefreshTokenExpiresAt ?? expiresAt
        };

        _logger.LogInformation("Admin session refreshed successfully for user '{Username}'", user.Username);
        return Result<LoginResultDto>.Success(response);
    }

    public async Task<Result<LoginResultDto>> RefreshTokenByTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result<LoginResultDto>.Unauthorized("Refresh token was not provided.");
        }

        var users = await _unitOfWork.Repository<AdminUser>()
            .FindAsync(u => u.RefreshToken == refreshToken && !u.IsDeleted && u.IsActive, ct);

        var user = users.FirstOrDefault();
        if (user == null || !user.RefreshTokenExpiresAt.HasValue || user.RefreshTokenExpiresAt.Value <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token validation failed: token invalid, expired, or user inactive.");
            return Result<LoginResultDto>.Unauthorized("Refresh token has expired or is invalid. Please log in again.");
        }

        // Issue new 5-minute JWT
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        // Rotate Refresh Token securely
        var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.RefreshToken = newRefreshToken;
        // Maintain existing expiration timestamp to preserve initial RememberMe duration window
        _unitOfWork.Repository<AdminUser>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Refresh token rotated successfully for admin user '{Username}'", user.Username);

        var userDto = new AdminUserDto
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            LastLoginAt = user.LastLoginAt
        };

        var response = new LoginResultDto
        {
            User = userDto,
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = user.RefreshTokenExpiresAt.Value
        };

        return Result<LoginResultDto>.Success(response);
    }

    public async Task<Result> ChangePasswordAsync(string username, ChangePasswordDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return Result.Failure("کلمه عبور فعلی و کلمه عبور جدید الزامی هستند.");
        }

        if (dto.NewPassword != dto.ConfirmNewPassword)
        {
            return Result.Failure("کلمه عبور جدید با تکرار آن مطابقت ندارد.");
        }

        // Validate password complexity rules (minimum 8 characters, letters, and numbers)
        if (dto.NewPassword.Length < 8)
        {
            return Result.Failure("کلمه عبور جدید باید حداقل ۸ کاراکتر باشد.");
        }

        if (!dto.NewPassword.Any(char.IsLetter) || !dto.NewPassword.Any(char.IsDigit))
        {
            return Result.Failure("کلمه عبور جدید باید شامل حداقل یک حرف و یک عدد باشد.");
        }

        var users = await _unitOfWork.Repository<AdminUser>()
            .FindAsync(u => u.Username == username && !u.IsDeleted && u.IsActive, ct);

        var user = users.FirstOrDefault();
        if (user == null)
        {
            return Result.Unauthorized("حساب کاربری مدیریت یافت نشد.");
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Password change failed for '{Username}': Current password was incorrect.", username);
            return Result.Failure("کلمه عبور فعلی نادرست است.");
        }

        // Hash new password using PBKDF2
        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);

        // Security best practice: Invalidate all active refresh tokens upon password change
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        _unitOfWork.Repository<AdminUser>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Password successfully changed and refresh tokens invalidated for admin user '{Username}'", username);
        return Result.Success();
    }

    public async Task<Result> LogoutAsync(string username, CancellationToken ct = default)
    {
        var users = await _unitOfWork.Repository<AdminUser>()
            .FindAsync(u => u.Username == username && !u.IsDeleted, ct);

        var user = users.FirstOrDefault();
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            _unitOfWork.Repository<AdminUser>().Update(user);
            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("Admin user '{Username}' logged out and refresh token revoked", username);
        }

        return Result.Success();
    }
}
