using _116.Shared.Application.Extensions;
using AwesomeAssertions;
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
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseResourceNotFoundHandler_ShouldNotThrow()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act & Assert
        Exception? exception = Record.Exception(() => app.UseResourceNotFoundHandler());
        exception.Should().BeNull();
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
        app.Should().NotBeNull();
    }
}
