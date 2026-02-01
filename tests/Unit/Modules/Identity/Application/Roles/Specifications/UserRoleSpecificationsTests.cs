using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Specifications;

/// <summary>
/// Unit tests for UserRole specifications.
/// </summary>
public class UserRoleSpecificationsTests
{
    #region UserHasAdminRoleSpecification Tests

    [Fact]
    public void UserHasAdminRoleSpecification_WithAdminRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity adminRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.Admin)).Build();
        UserEntity user = new UserBuilder().WithRole(adminRole).Build();
        UserHasAdminRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserHasAdminRoleSpecification_WithSuperAdminRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity superAdminRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.SuperAdmin)).Build();
        UserEntity user = new UserBuilder().WithRole(superAdminRole).Build();
        UserHasAdminRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserHasAdminRoleSpecification_WithVisitorRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity visitorRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.Visitor)).Build();
        UserEntity user = new UserBuilder().WithRole(visitorRole).Build();
        UserHasAdminRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserHasAdminRoleSpecification_WithNoRoles_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().Build();
        UserHasAdminRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserHasRoleSpecification Tests

    [Fact]
    public void UserHasRoleSpecification_WithMatchingRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity moderatorRole = new RoleBuilder().WithName("Moderator").Build();
        UserEntity user = new UserBuilder().WithRole(moderatorRole).Build();
        UserHasRoleSpecification spec = new("Moderator");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserHasRoleSpecification_WithDifferentRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity visitorRole = new RoleBuilder().WithName("Visitor").Build();
        UserEntity user = new UserBuilder().WithRole(visitorRole).Build();
        UserHasRoleSpecification spec = new("Admin");

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserHasVisitorRoleSpecification Tests

    [Fact]
    public void UserHasVisitorRoleSpecification_WithVisitorRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity visitorRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.Visitor)).Build();
        UserEntity user = new UserBuilder().WithRole(visitorRole).Build();
        UserHasVisitorRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserHasVisitorRoleSpecification_WithAdminRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity adminRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.Admin)).Build();
        UserEntity user = new UserBuilder().WithRole(adminRole).Build();
        UserHasVisitorRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserIsActiveAdminSpecification Tests

    [Fact]
    public void UserIsActiveAdminSpecification_WithActiveAdminUser_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity adminRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.Admin)).Build();
        UserEntity user = new UserBuilder().WithRole(adminRole).AsActive().Build();
        UserIsActiveAdminSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserIsActiveAdminSpecification_WithActiveSuperAdminUser_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity superAdminRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.SuperAdmin)).Build();
        UserEntity user = new UserBuilder().WithRole(superAdminRole).AsActive().Build();
        UserIsActiveAdminSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserIsActiveAdminSpecification_WithInactiveAdminUser_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity adminRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.Admin)).Build();
        UserEntity user = new UserBuilder().WithRole(adminRole).AsInactive().Build();
        UserIsActiveAdminSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserIsActiveAdminSpecification_WithActiveVisitorUser_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity visitorRole = new RoleBuilder().WithName(nameof(EnumCoreUserRole.Visitor)).Build();
        UserEntity user = new UserBuilder().WithRole(visitorRole).AsActive().Build();
        UserIsActiveAdminSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserIsActiveAdminSpecification_WithInactiveUserNoRoles_ShouldReturnFalse()
    {
        // Arrange
        UserEntity user = new UserBuilder().AsInactive().Build();
        UserIsActiveAdminSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserRoleByUserIdSpecification Tests

    [Fact]
    public void UserRoleByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).Build();
        UserRoleByUserIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(userRole);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserRoleByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(Guid.NewGuid()).Build();
        UserRoleByUserIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(userRole);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserRoleByRoleIdSpecification Tests

    [Fact]
    public void UserRoleByRoleIdSpecification_WithMatchingRoleId_ShouldReturnTrue()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        UserRoleEntity userRole = new UserRoleBuilder().WithRoleId(roleId).Build();
        UserRoleByRoleIdSpecification spec = new(roleId);

        // Act
        bool result = spec.IsSatisfiedBy(userRole);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserRoleByRoleIdSpecification_WithDifferentRoleId_ShouldReturnFalse()
    {
        // Arrange
        UserRoleEntity userRole = new UserRoleBuilder().WithRoleId(Guid.NewGuid()).Build();
        UserRoleByRoleIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(userRole);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserRoleByUserAndRoleSpecification Tests

    [Fact]
    public void UserRoleByUserAndRoleSpecification_WithMatchingBoth_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(userId).WithRoleId(roleId).Build();
        UserRoleByUserAndRoleSpecification spec = new(userId, roleId);

        // Act
        bool result = spec.IsSatisfiedBy(userRole);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserRoleByUserAndRoleSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        UserRoleEntity userRole = new UserRoleBuilder().WithUserId(Guid.NewGuid()).WithRoleId(roleId).Build();
        UserRoleByUserAndRoleSpecification spec = new(Guid.NewGuid(), roleId);

        // Act
        bool result = spec.IsSatisfiedBy(userRole);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
