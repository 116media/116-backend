using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities;
using _116.Tests.Fixtures.Factories;
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
        RoleEntity adminRole = RoleFactory.CreateAdmin();
        UserEntity user = UserFactory.CreateWithRole(adminRole);
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
        RoleEntity superAdminRole = RoleFactory.CreateSuperAdmin();
        UserEntity user = UserFactory.CreateWithRole(superAdminRole);
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
        RoleEntity visitorRole = RoleFactory.CreateVisitor();
        UserEntity user = UserFactory.CreateWithRole(visitorRole);
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
        UserEntity user = UserFactory.Create();
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
        RoleEntity moderatorRole = RoleFactory.Create("Moderator");
        UserEntity user = UserFactory.CreateWithRole(moderatorRole);
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
        RoleEntity visitorRole = RoleFactory.Create("Visitor");
        UserEntity user = UserFactory.CreateWithRole(visitorRole);
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
        RoleEntity visitorRole = RoleFactory.CreateVisitor();
        UserEntity user = UserFactory.CreateWithRole(visitorRole);
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
        RoleEntity adminRole = RoleFactory.CreateAdmin();
        UserEntity user = UserFactory.CreateWithRole(adminRole);
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
        RoleEntity adminRole = RoleFactory.CreateAdmin();
        UserEntity user = UserFactory.CreateWithRole(adminRole);
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
        RoleEntity superAdminRole = RoleFactory.CreateSuperAdmin();
        UserEntity user = UserFactory.CreateWithRole(superAdminRole);
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
        RoleEntity adminRole = RoleFactory.CreateAdmin();
        UserEntity user = UserFactory.CreateInactive();
        user.AssignRole(UserRoleFactory.Create(user.Id, adminRole.Id));
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
        RoleEntity visitorRole = RoleFactory.CreateVisitor();
        UserEntity user = UserFactory.CreateWithRole(visitorRole);
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
        UserEntity user = UserFactory.CreateInactive();
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
        UserRoleEntity userRole = UserRoleFactory.CreateWithUserId(userId);
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
        UserRoleEntity userRole = UserRoleFactory.CreateWithUserId(Guid.NewGuid());
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
        UserRoleEntity userRole = UserRoleFactory.CreateWithRoleId(roleId);
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
        UserRoleEntity userRole = UserRoleFactory.CreateWithRoleId(Guid.NewGuid());
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
        UserRoleEntity userRole = UserRoleFactory.Create(userId, roleId);
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
        UserRoleEntity userRole = UserRoleFactory.Create(Guid.NewGuid(), roleId);
        UserRoleByUserAndRoleSpecification spec = new(Guid.NewGuid(), roleId);

        // Act
        bool result = spec.IsSatisfiedBy(userRole);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
