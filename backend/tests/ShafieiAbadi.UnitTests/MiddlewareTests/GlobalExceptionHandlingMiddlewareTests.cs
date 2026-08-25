using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ShafieiAbadi.Api.Middlewares;
using ShafieiAbadi.Application.Common;

namespace ShafieiAbadi.UnitTests.MiddlewareTests;

public class GlobalExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenExceptionOccurs_ShouldReturn500AndStandardizedResultJson()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        RequestDelegate next = (HttpContext _) => throw new InvalidOperationException("خطای تست پایگاه داده");

        var middleware = new GlobalExceptionHandlingMiddleware(next, mockLogger.Object, mockEnv.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().Contain("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        var result = JsonSerializer.Deserialize<Result>(responseBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.ErrorMessage.Should().Contain("خطای تست پایگاه داده");
    }
}
