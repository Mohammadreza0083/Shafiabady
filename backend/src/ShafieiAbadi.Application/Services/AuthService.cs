using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Admin login attempt with empty credentials");
            return Result<LoginResponseDto>.Failure("نام کاربری و گذرواژه الزامی است.");
        }

        var users = await _unitOfWork.Repository<AdminUser>()
            .FindAsync(u => (u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail) && !u.IsDeleted && u.IsActive, ct);

        var user = users.FirstOrDefault();
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for non-existing or inactive user '{Username}'", request.UsernameOrEmail);
            return Result<LoginResponseDto>.Unauthorized("اطلاعات ورود نادرست است.");
        }

        // Verify SHA256 password hash (or fallback check for initial seeding)
        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash && user.PasswordHash != request.Password)
        {
            _logger.LogWarning("Failed login attempt (invalid password) for user '{Username}'", request.UsernameOrEmail);
            return Result<LoginResponseDto>.Unauthorized("اطلاعات ورود نادرست است.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Repository<AdminUser>().Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Admin user '{Username}' logged in successfully", user.Username);

        var response = new LoginResponseDto
        {
            Token = "jwt_mock_token_" + Guid.NewGuid().ToString("N"),
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        return Result<LoginResponseDto>.Success(response);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
