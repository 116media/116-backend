using _116.Identity.Application.Shared.Authorizations.Requirements;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Authorizations.Requirements;

/// <summary>
/// Unit tests for <see cref="UserRoleRequirement"/>.
/// </summary>
public class UserRoleRequirementTests
{
    [Fact]
    public void Constructor_WithValidRoles_ShouldSetAllowedRoles()
    {
        // Arrange
        string[] roles = ["Admin", "SuperAdmin"];

        // Act
        var requirement = new UserRoleRequirement(roles);

        // Assert
        requirement.AllowedRoles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void Constructor_WithNullAllowedRoles_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new UserRoleRequirement(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("allowedRoles");
    }

    [Fact]
    public void Constructor_WithEmptyRoles_ShouldSetEmptyAllowedRoles()
    {
        // Act
        var requirement = new UserRoleRequirement([]);

        // Assert
        requirement.AllowedRoles.Should().BeEmpty();
    }
}
