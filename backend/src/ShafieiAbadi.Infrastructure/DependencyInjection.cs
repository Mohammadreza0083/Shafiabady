using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShafieiAbadi.Application.Interfaces;
using ShafieiAbadi.Infrastructure.Authentication;
using ShafieiAbadi.Infrastructure.BackgroundServices;
using ShafieiAbadi.Infrastructure.Data;
using ShafieiAbadi.Infrastructure.Repositories;

namespace ShafieiAbadi.Infrastructure;

/// <summary>
/// Dependency injection extensions for registering infrastructure data access, background workers, and security services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemoryConfig = configuration["UseInMemoryDatabase"];
        var isInMemory = bool.TryParse(useInMemoryConfig, out var parsed) && parsed;
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=sqlserver,1433;Database=ShafieiAbadiDb;User Id=sa;Password=P@ssw0rd2026!DrShafiei;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        if (isInMemory)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("ShafieiAbadiDb"));
        }
        else
        {
            // Strictly configure Microsoft SQL Server with connection retry resilience
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));
        }

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<DatabaseSeeder>();

        // Register recurring background worker for purging rejected comments older than 7 days
        services.AddHostedService<RejectedCommentsPurgeBackgroundService>();

        return services;
    }
}
