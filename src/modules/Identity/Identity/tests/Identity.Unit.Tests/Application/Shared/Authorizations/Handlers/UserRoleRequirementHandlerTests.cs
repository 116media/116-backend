using System.Security.Claims;
using _116.Identity.Application.Shared.Authorizations.Handlers;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Shared.Authorizations.Handlers;

public class UserRoleRequirementHandlerTests
{
    private readonly UserRoleRequirementHandler _handler = new();
    private static readonly string[] AdminRoles = ["Admin", "SuperAdmin"];
    private static readonly string[] AdminOnly = ["Admin"];
    private static readonly string[] AdminRolesWithModerator = ["Admin", "SuperAdmin", "Moderator"];

    [Fact]
    public async Task HandleRequirementAsync_WithMatchingRole_ShouldSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var context = new AuthorizationHandlerContext([new UserRoleRequirement(AdminRoles)], user, null);
        var requirement = new UserRoleRequirement(AdminRoles);

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
        var requirement = new UserRoleRequirement(AdminOnly);
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
        var requirement = new UserRoleRequirement(AdminRoles);
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
        var requirement = new UserRoleRequirement(AdminOnly);
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
        var requirement = new UserRoleRequirement(AdminRolesWithModerator);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithAllowedRoleNotFirstClaim_ShouldSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "Visitor"), new Claim(ClaimTypes.Role, "Admin") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserRoleRequirement(AdminRoles);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleRolesNoneAllowed_ShouldNotSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.Role, "Visitor"), new Claim(ClaimTypes.Role, "Moderator") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserRoleRequirement(AdminRoles);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
