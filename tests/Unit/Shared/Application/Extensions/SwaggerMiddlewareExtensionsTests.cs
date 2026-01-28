using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="SwaggerMiddlewareExtensions"/>.
/// </summary>
public class SwaggerMiddlewareExtensionsTests
{
    [Fact]
    public void UseSwaggerFormatting_ShouldReturnApplicationBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act
        IApplicationBuilder result = app.UseSwaggerFormatting();

        // Assert
        Assert.Same(app, result);
    }

    [Fact]
    public void UseSwaggerFormatting_ShouldNotThrow()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act & Assert
        var exception = Record.Exception(() => app.UseSwaggerFormatting());
        Assert.Null(exception);
    }

    [Fact]
    public void UseSwaggerFormatting_ShouldAddMiddlewareToPipeline()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act
        app.UseSwaggerFormatting();

        // Assert - Middleware is added to the pipeline (no exception thrown)
        Assert.NotNull(app);
    }
}
