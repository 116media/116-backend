using System.Security.Claims;
using _116.Identity.Application.Shared.Authorizations.Handlers;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Authorizations.Handlers;

public class UserRoleRequirementHandlerTests
{
    private readonly UserRoleRequirementHandler _handler = new();

    [Fact]
    public async Task HandleRequirementAsync_WithMatchingRole_ShouldSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var context = new AuthorizationHandlerContext(
            [new UserRoleRequirement(new[] { "Admin", "SuperAdmin" })],
            user,
            null
        );
        var requirement = new UserRoleRequirement(new[] { "Admin", "SuperAdmin" });

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithCaseInsensitiveMatch_ShouldSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "admin") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserRoleRequirement(new[] { "Admin" });
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNonMatchingRole_ShouldNotSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "Visitor") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserRoleRequirement(new[] { "Admin", "SuperAdmin" });
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoRoleClaim_ShouldNotSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new UserRoleRequirement(new[] { "Admin" });
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleAllowedRoles_ShouldSucceedOnMatch()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "SuperAdmin") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserRoleRequirement(new[] { "Admin", "SuperAdmin", "Moderator" });
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
}
