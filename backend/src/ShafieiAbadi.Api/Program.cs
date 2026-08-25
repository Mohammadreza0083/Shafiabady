using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

// 2. Add Layer Services (Application & Infrastructure)
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

// 5. Configure CORS Policy
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? new[]
{
    "http://localhost:4500",
    "http://localhost:4321",
    "http://localhost:3000",
    "http://127.0.0.1:4500",
    "http://127.0.0.1:4321"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
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

// 7. Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 8. Serilog HTTP Request Logging
app.UseSerilogRequestLogging();

// 9. Swagger & Swagger UI Setup
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

// 10. CORS, Authentication, and Authorization Middleware Pipeline
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 11. Seed Initial Database Records (Admin user, categories, sample articles, books, and courses)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}

try
{
    Log.Information("Starting ShafieiAbadi.Api Web Host on port {Port}...", builder.Configuration["PORT"] ?? "5000");
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
