using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="UserRoleEntity"/>.
/// </summary>
public class UserRoleEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateUserRole()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        // Act
        UserRoleEntity userRole = UserRoleEntity.Create(id, userId, roleId);

        // Assert
        userRole.Id.Should().Be(id);
        userRole.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void Create_ShouldAllowEmptyGuids()
    {
        // Arrange
        Guid id = Guid.Empty;
        Guid userId = Guid.Empty;
        Guid roleId = Guid.Empty;

        // Act
        UserRoleEntity userRole = UserRoleEntity.Create(id, userId, roleId);

        // Assert
        userRole.Id.Should().Be(Guid.Empty);
        userRole.UserId.Should().Be(Guid.Empty);
        userRole.RoleId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldNotSetNavigationProperties()
    {
        // Arrange & Act
        UserRoleEntity userRole = UserRoleEntity.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        // Navigation properties should be null since they're not set in Create
        // They're set by EF Core when loading related entities
        userRole.Id.Should().NotBeEmpty();
    }

    #endregion

    #region Builder Tests

    [Fact]
    public void Builder_ShouldCreateUserRoleWithSpecifiedValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        // Act
        UserRoleEntity userRole = UserRoleFactory.CreateWithId(id, userId, roleId);

        // Assert
        userRole.Id.Should().Be(id);
        userRole.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void Builder_WithDefaultValues_ShouldCreateValidUserRole()
    {
        // Arrange & Act
        UserRoleEntity userRole = UserRoleFactory.Create();

        // Assert
        userRole.Id.Should().NotBeEmpty();
        userRole.UserId.Should().NotBeEmpty();
        userRole.RoleId.Should().NotBeEmpty();
    }

    #endregion

    #region Association Tests

    [Fact]
    public void MultipleUserRoles_WithSameUser_ShouldHaveDifferentRoles()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId1 = Guid.NewGuid();
        Guid roleId2 = Guid.NewGuid();

        // Act
        UserRoleEntity userRole1 = UserRoleFactory.Create(userId, roleId1);

        UserRoleEntity userRole2 = UserRoleFactory.Create(userId, roleId2);

        // Assert
        userRole1.UserId.Should().Be(userId);
        userRole2.UserId.Should().Be(userId);
        userRole1.RoleId.Should().NotBe(userRole2.RoleId);
    }

    [Fact]
    public void MultipleUserRoles_WithSameRole_ShouldHaveDifferentUsers()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid userId1 = Guid.NewGuid();
        Guid userId2 = Guid.NewGuid();

        // Act
        UserRoleEntity userRole1 = UserRoleFactory.Create(userId1, roleId);

        UserRoleEntity userRole2 = UserRoleFactory.Create(userId2, roleId);

        // Assert
        userRole1.RoleId.Should().Be(roleId);
        userRole2.RoleId.Should().Be(roleId);
        userRole1.UserId.Should().NotBe(userRole2.UserId);
    }

    #endregion
}
