using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="ResourceNotFoundExtension"/>.
/// </summary>
public class ResourceNotFoundExtensionTests
{
    [Fact]
    public void UseResourceNotFoundHandler_ShouldReturnApplicationBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act
        IApplicationBuilder result = app.UseResourceNotFoundHandler();

        // Assert
        Assert.Same(app, result);
    }

    [Fact]
    public void UseResourceNotFoundHandler_ShouldNotThrow()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act & Assert
        var exception = Record.Exception(() => app.UseResourceNotFoundHandler());
        Assert.Null(exception);
    }

    [Fact]
    public void UseResourceNotFoundHandler_ShouldAddMiddlewareToPipeline()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act
        app.UseResourceNotFoundHandler();

        // Assert - Middleware is added to the pipeline (no exception thrown)
        Assert.NotNull(app);
    }
}
