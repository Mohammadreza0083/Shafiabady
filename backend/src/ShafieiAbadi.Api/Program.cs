using System.Reflection;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using ShafieiAbadi.Api.Middlewares;
using ShafieiAbadi.Application;
using ShafieiAbadi.Infrastructure;
using ShafieiAbadi.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog & Grafana Loki structured logging
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

// Enforce port 8200 strictly
builder.WebHost.UseUrls("http://localhost:8200");

// Add Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Controllers
builder.Services.AddControllers();

// Configure CORS
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

// Configure Swagger / OpenAPI with XML Documentation Comments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "سامانه وب و برند شخصی دکتر محمد حسین شفیع‌آبادی",
        Version = "v1",
        Description = "مجموعه APIهای مدیریت محتوا، مقالات، کتاب‌ها، دوره‌های آموزشی و آمار بر اساس Clean Architecture و DDD در .NET 9",
        Contact = new OpenApiContact
        {
            Name = "دکتر محمد حسین شفیع‌آبادی",
            Url = new Uri("http://localhost:4500")
        }
    });

    // Load XML Documentation file
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Serilog Request Logging
app.UseSerilogRequestLogging();

// Swagger UI
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dr. ShafieiAbadi API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "مستندات API دکتر محمد حسین شفیع‌آبادی";
    });
}

app.UseCors("DefaultCorsPolicy");

app.UseAuthorization();

app.MapControllers();

// Seed initial database records
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}

try
{
    Log.Information("Starting ShafieiAbadi.Api Web Host on port 8200...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
