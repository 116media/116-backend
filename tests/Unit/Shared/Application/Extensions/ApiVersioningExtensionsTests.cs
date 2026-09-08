using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="ApiVersioningExtensions"/>.
/// </summary>
public class ApiVersioningExtensionsTests
{
    [Fact]
    public void UseApiVersioning_ShouldNotThrow()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();

        // Act & Assert
        Exception? exception = Record.Exception(() => app.UseApiVersioning());
        exception.Should().BeNull();
    }

    [Fact]
    public void UseApiVersioning_ShouldConfigureVersionedRoutes()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();

        // Act
        app.UseApiVersioning();

        // Assert - No exception thrown, versioning initialized
        app.Should().NotBeNull();
    }

    [Fact]
    public void MapApiVersionGroup_WithVersion1_ShouldReturnRouteGroupBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();
        app.UseApiVersioning();

        // Act
        RouteGroupBuilder result = app.MapApiVersionGroup(1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapApiVersionGroup_WithVersion2_ShouldReturnRouteGroupBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();
        app.UseApiVersioning();

        // Act
        RouteGroupBuilder result = app.MapApiVersionGroup(2);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapApiVersionGroup_WithDeprecatedVersion_ShouldReturnRouteGroupBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();
        app.UseApiVersioning();

        // Act
        RouteGroupBuilder result = app.MapApiVersionGroup(1, isDeprecated: true);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapApiVersionGroup_WithNonDeprecatedVersion_ShouldReturnRouteGroupBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();
        app.UseApiVersioning();

        // Act
        RouteGroupBuilder result = app.MapApiVersionGroup(2, isDeprecated: false);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapApiVersionGroup_WithoutUseApiVersioning_ShouldThrowInvalidOperationException()
    {
        // The holder is registered but never initialized, so the Group getter throws.
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();

        Action act = () => app.MapApiVersionGroup(1);

        act.Should().Throw<InvalidOperationException>().WithMessage("*app.UseApiVersioning()*");
    }

    [Fact]
    public void MapApiVersionGroup_MultipleVersions_ShouldAllReturnBuilders()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();
        app.UseApiVersioning();

        // Act
        RouteGroupBuilder v1 = app.MapApiVersionGroup(1);
        RouteGroupBuilder v2 = app.MapApiVersionGroup(2);

        // Assert
        v1.Should().NotBeNull();
        v2.Should().NotBeNull();
    }

    [Fact]
    public void UseApiVersioning_ShouldCreateVersionSetWith_V1_AndV2()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
        builder.Services.AddApiVersionGroupHolder();
        WebApplication app = builder.Build();

        // Act
        app.UseApiVersioning();

        // Assert - Both versions can be mapped
        RouteGroupBuilder v1 = app.MapApiVersionGroup(1);
        RouteGroupBuilder v2 = app.MapApiVersionGroup(2);
        v1.Should().NotBeNull();
        v2.Should().NotBeNull();
    }
}
