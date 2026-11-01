using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using ShafieiAbadi.Api.Middlewares;
using ShafieiAbadi.Application;
using ShafieiAbadi.Infrastructure;
using ShafieiAbadi.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog & Grafana Loki structured logging
var lokiUrl = builder.Configuration["Loki:Uri"] ?? "http://localhost:3100";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "ShafieiAbadi.Api")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.GrafanaLoki(lokiUrl)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add Forwarded Headers for Cloudflare & Reverse Proxy real IP resolution
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 3. Add Health Checks & Metrics
builder.Services.AddHealthChecks();

// 4. Add Layer Services (Application & Infrastructure)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Configure JWT Bearer Authentication with HttpOnly Cookie extraction
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "DrShafieiAbadi_SuperSecretKey_ProductionGrade_2026_DotNet9_CleanArch!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ShafieiAbadi.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ShafieiAbadi.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Extract JWT from HttpOnly cookie 'AdminAuthToken' or Authorization Bearer header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("AdminAuthToken", out var cookieToken) && !string.IsNullOrWhiteSpace(cookieToken))
            {
                context.Token = cookieToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// 4. Add Controllers
builder.Services.AddControllers();

// 5. Configure Built-in ASP.NET Core Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed-window rate limiter for sensitive authentication endpoints (5 requests per 1 minute)
    options.AddFixedWindowLimiter("auth-rate-limit", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Sliding-window limiter for general API traffic
    options.AddSlidingWindowLimiter("global-sliding", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 4;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

// 6. Configure Production-Hardened CORS Policy
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? new[]
{
    "https://shafieiabadi.ir",
    "https://www.shafieiabadi.ir",
    "http://localhost:4500",
    "http://localhost:4321",
    "http://localhost:3000",
    "http://127.0.0.1:4500",
    "http://127.0.0.1:4321",
    "http://localhost:80",
    "http://127.0.0.1:80"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;

                // Local development origins
                if (uri.Host == "localhost" || uri.Host == "127.0.0.1") return true;

                // Production domain & subdomains
                if (uri.Host == "shafieiabadi.ir" || uri.Host.EndsWith(".shafieiabadi.ir")) return true;

                // Vercel preview environments
                if (uri.Host.EndsWith(".vercel.app")) return true;

                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 6. Configure Swagger / OpenAPI with Professional English Metadata & XML Comments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dr. Mohammad Hossein Shafiei Abadi - API",
        Version = "v1",
        Description = "RESTful API for the Personal Branding & Educational Platform of Dr. Mohammad Hossein Shafiei Abadi, built with .NET 9, Clean Architecture, and Domain-Driven Design (DDD).",
        Contact = new OpenApiContact
        {
            Name = "Dr. Mohammad Hossein Shafiei Abadi",
            Url = new Uri("http://localhost:4500")
        }
    });

    // Add JWT Bearer / Cookie Security Definition in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme or automatic HttpOnly cookie. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Load XML Documentation Comments from API, Application, and Domain assemblies
    var assemblies = new[]
    {
        Assembly.GetExecutingAssembly(),
        typeof(ShafieiAbadi.Application.DependencyInjection).Assembly,
        typeof(ShafieiAbadi.Domain.Common.BaseEntity).Assembly
    };

    foreach (var assembly in assemblies)
    {
        var xmlFile = $"{assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }
});

var app = builder.Build();

// 7. Process Forwarded Headers (Cloudflare real IP & TLS termination)
app.UseForwardedHeaders();

// 8. Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 8.1 Serve admin-uploaded images (covers, thumbnails, inline content images) from wwwroot/uploads
var uploadsRoot = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads");
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsRoot),
    RequestPath = ShafieiAbadi.Api.Controllers.UploadsController.PublicPathPrefix,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
    }
});

// 9. Serilog HTTP Request Logging & Prometheus Metrics
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseHttpMetrics();

// 10. Swagger & Swagger UI Setup
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dr. Mohammad Hossein Shafiei Abadi - API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Dr. Mohammad Hossein Shafiei Abadi - API Documentation";
    });
}

// 11. CORS, Rate Limiting, Authentication, and Authorization Middleware Pipeline
app.UseCors("DefaultCorsPolicy");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// 12. Health Check and API Controllers Endpoints
app.MapHealthChecks("/health");
app.MapControllers();

// 13. Database Migration & Seed Initial Database Records with Resilient Retry Loop
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ShafieiAbadi.Infrastructure.Data.AppDbContext>();

    if (dbContext.Database.IsRelational())
    {
        const int maxRetryAttempts = 5;
        var retryDelay = TimeSpan.FromSeconds(3);
        var migrationSucceeded = false;

        for (var attempt = 1; attempt <= maxRetryAttempts; attempt++)
        {
            try
            {
                Log.Information("Applying pending SQL Server database migrations (Attempt {Attempt}/{MaxAttempts})...", attempt, maxRetryAttempts);
                await dbContext.Database.MigrateAsync();
                Log.Information("SQL Server database migrations applied successfully.");
                migrationSucceeded = true;
                break;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to connect to SQL Server or apply migrations on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}s...", attempt, maxRetryAttempts, retryDelay.TotalSeconds);
                if (attempt < maxRetryAttempts)
                {
                    await Task.Delay(retryDelay);
                }
            }
        }

        if (!migrationSucceeded)
        {
            Log.Fatal("Critical failure: Could not apply SQL Server migrations after {MaxAttempts} attempts. Halting startup.", maxRetryAttempts);
            throw new InvalidOperationException("Failed to connect to SQL Server and apply migrations. Database is unreachable.");
        }
    }

    try
    {
        Log.Information("Executing initial database seeding...");
        var seeder = services.GetRequiredService<ShafieiAbadi.Infrastructure.Data.DatabaseSeeder>();
        await seeder.SeedAsync();
        Log.Information("Initial database seeding finished successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while executing database seeder.");
        throw;
    }
}

// 14. Serve Prometheus Metrics on a separate internal port (not published outside the Docker network)
var metricsPort = builder.Configuration.GetValue("Metrics:Port", 9091);
using var metricServer = new KestrelMetricServer(port: metricsPort);
metricServer.Start();

try
{
    Log.Information("Starting ShafieiAbadi.Api Web Host on port {Port} (metrics on internal port {MetricsPort})...", builder.Configuration["PORT"] ?? "5000", metricsPort);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ShafieiAbadi.Api host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
