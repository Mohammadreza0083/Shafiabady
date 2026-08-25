using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Infrastructure.Authentication;

/// <summary>
/// Generates cryptographically signed JSON Web Tokens (JWT) for authenticated administrators with strict 5-minute lifespan.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(AdminUser adminUser)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "DrShafieiAbadi_SuperSecretKey_ProductionGrade_2026_DotNet9_CleanArch!";
        var issuer = _configuration["Jwt:Issuer"] ?? "ShafieiAbadi.Api";
        var audience = _configuration["Jwt:Audience"] ?? "ShafieiAbadi.Client";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, adminUser.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, adminUser.Username),
            new(JwtRegisteredClaimNames.Email, adminUser.Email),
            new(ClaimTypes.Name, adminUser.Username),
            new(ClaimTypes.Role, "Admin"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Strict 5-minute lifespan
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        return (tokenString, expiresAt);
    }
}
