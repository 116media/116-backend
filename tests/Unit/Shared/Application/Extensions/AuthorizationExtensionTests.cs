using _116.Shared.Application.Extensions;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="AuthorizationExtension"/>.
/// </summary>
public class AuthorizationExtensionTests
{
    [Fact]
    public void WithAuthorization_ShouldReturnSameBuilder()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();
        RouteGroupBuilder group = app.MapGroup("/test");

        // Act
        RouteGroupBuilder result = group.WithAuthorization("AdminOnly");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(group);
    }

    [Theory]
    [InlineData("AdminOnly")]
    [InlineData("SuperAdminOnly")]
    [InlineData("RequireAdminOrSuperAdmin")]
    public void WithAuthorization_WithDifferentPolicies_ShouldNotThrow(string policy)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();
        RouteGroupBuilder group = app.MapGroup("/test");

        // Act
        Exception? exception = Record.Exception(() => group.WithAuthorization(policy));

        // Assert
        exception.Should().BeNull();
    }
}
