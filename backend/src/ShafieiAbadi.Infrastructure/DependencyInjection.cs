using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Infrastructure.Authentication;
using ShafieiAbadi.Infrastructure.Data;
using ShafieiAbadi.Infrastructure.Repositories;

namespace ShafieiAbadi.Infrastructure;

/// <summary>
/// Dependency injection extensions for registering infrastructure data access and security services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemoryConfig = configuration["UseInMemoryDatabase"];
        var isInMemory = bool.TryParse(useInMemoryConfig, out var parsed) && parsed;
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!isInMemory && !string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("ShafieiAbadiDb"));
        }

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
