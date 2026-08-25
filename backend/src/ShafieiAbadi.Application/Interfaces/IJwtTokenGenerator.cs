using ShafieiAbadi.Domain.Entities;

namespace ShafieiAbadi.Application.Interfaces;

/// <summary>
/// Service contract for generating secure JSON Web Tokens (JWT) for authenticated administrators.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a signed JWT for the given administrator with a strict 5-minute lifespan.
    /// </summary>
    /// <param name="adminUser">The authenticated administrator domain entity.</param>
    /// <returns>A tuple containing the raw JWT string and the expiration UTC timestamp.</returns>
    (string Token, DateTime ExpiresAt) GenerateToken(AdminUser adminUser);
}
