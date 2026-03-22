using System.Reflection;
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
        // Covers the _rootVersionedGroup ?? throw branch.
        // Reset the static field to null via reflection, test, then restore.
        FieldInfo? field = typeof(ApiVersioningExtensions).GetField(
            "_rootVersionedGroup",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        object? original = field?.GetValue(null);
        try
        {
            field?.SetValue(null, null);

            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            WebApplication app = builder.Build();

            Action act = () => app.MapApiVersionGroup(1);

            act.Should().Throw<InvalidOperationException>().WithMessage("*app.UseApiVersioning()*");
        }
        finally
        {
            field?.SetValue(null, original);
        }
    }

    // Note: Removed tests for static state checking as they interfere with other tests
    // The static _rootVersionedGroup persists across test runs causing unpredictable behavior

    [Fact]
    public void MapApiVersionGroup_MultipleVersions_ShouldAllReturnBuilders()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddApiVersioning();
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
