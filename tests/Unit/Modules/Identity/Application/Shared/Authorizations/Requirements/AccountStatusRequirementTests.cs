using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Authorizations.Requirements;

/// <summary>
/// Unit tests for <see cref="AccountStatusRequirement"/>.
/// </summary>
public class AccountStatusRequirementTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldSetProperties()
    {
        // Arrange
        string claimType = JwtClaimsConstants.IsActive;
        string claimValue = "true";

        // Act
        var requirement = new AccountStatusRequirement(claimType, claimValue);

        // Assert
        requirement.ClaimType.Should().Be(claimType);
        requirement.ClaimValue.Should().Be(claimValue);
    }

    [Fact]
    public void Constructor_WithNullClaimType_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new AccountStatusRequirement(null!, "true");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("claimType");
    }

    [Fact]
    public void Constructor_WithNullClaimValue_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new AccountStatusRequirement(JwtClaimsConstants.IsActive, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("claimValue");
    }
}
