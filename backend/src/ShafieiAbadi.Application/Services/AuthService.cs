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
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
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

        // Verify SHA256 password hash (or fallback check for initial seeded admin)
        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash && user.PasswordHash != request.Password)
        {
            _logger.LogWarning("Failed login attempt (invalid password) for user '{Username}'", request.UsernameOrEmail);
            return Result<LoginResultDto>.Unauthorized("Invalid username or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Repository<AdminUser>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Admin user '{Username}' logged in successfully", user.Username);

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
            ExpiresAt = expiresAt
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

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
