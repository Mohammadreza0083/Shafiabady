using System.Net;
using System.Text.Json;
using ShafieiAbadi.Application.Common;

namespace ShafieiAbadi.Api.Middlewares;

/// <summary>
/// Global exception handling middleware capturing unhandled exceptions and returning standardized 500 error results.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request: {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;

        var message = _env.IsDevelopment()
            ? $"Internal Server Error: {exception.Message}"
            : "An unexpected error occurred on the server. Please contact the administrator.";

        var problemDetails = new
        {
            type = "https://datatracker.ietf.org/doc/html/rfc7807",
            title = _env.IsDevelopment() ? $"Internal Server Error: {exception.GetType().Name}" : "Internal Server Error",
            status = StatusCodes.Status500InternalServerError,
            detail = message,
            instance = context.Request.Path.Value,
            traceId,
            // Result envelope compatibility for frontend apiFetch and unit tests
            isSuccess = false,
            statusCode = StatusCodes.Status500InternalServerError,
            errorMessage = message,
            validationErrors = Array.Empty<string>()
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        return context.Response.WriteAsync(json);
    }
}
