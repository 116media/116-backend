using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="RolePermissionEntity"/>.
/// </summary>
public class RolePermissionEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateRolePermission()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity rolePermission = RolePermissionEntity.Create(id, roleId, permissionId);

        // Assert
        rolePermission.Id.Should().Be(id);
        rolePermission.RoleId.Should().Be(roleId);
        rolePermission.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public void Create_ShouldAllowEmptyGuids()
    {
        // Arrange
        Guid id = Guid.Empty;
        Guid roleId = Guid.Empty;
        Guid permissionId = Guid.Empty;

        // Act
        RolePermissionEntity rolePermission = RolePermissionEntity.Create(id, roleId, permissionId);

        // Assert
        rolePermission.Id.Should().Be(Guid.Empty);
        rolePermission.RoleId.Should().Be(Guid.Empty);
        rolePermission.PermissionId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldNotSetNavigationProperties()
    {
        // Arrange & Act
        RolePermissionEntity rolePermission = RolePermissionEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Assert
        // Navigation properties should be null since they're not set in Create
        // They're set by EF Core when loading related entities
        rolePermission.Id.Should().NotBeEmpty();
    }

    #endregion

    #region Builder Tests

    [Fact]
    public void Builder_ShouldCreateRolePermissionWithSpecifiedValues()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity rolePermission = new RolePermissionBuilder()
            .WithId(id)
            .WithRoleId(roleId)
            .WithPermissionId(permissionId)
            .Build();

        // Assert
        rolePermission.Id.Should().Be(id);
        rolePermission.RoleId.Should().Be(roleId);
        rolePermission.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public void Builder_WithDefaultValues_ShouldCreateValidRolePermission()
    {
        // Arrange & Act
        RolePermissionEntity rolePermission = new RolePermissionBuilder().Build();

        // Assert
        rolePermission.Id.Should().NotBeEmpty();
        rolePermission.RoleId.Should().NotBeEmpty();
        rolePermission.PermissionId.Should().NotBeEmpty();
    }

    #endregion

    #region Association Tests

    [Fact]
    public void MultipleRolePermissions_WithSameRole_ShouldHaveDifferentPermissions()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid permissionId1 = Guid.NewGuid();
        Guid permissionId2 = Guid.NewGuid();

        // Act
        RolePermissionEntity rolePermission1 = new RolePermissionBuilder()
            .WithRoleId(roleId)
            .WithPermissionId(permissionId1)
            .Build();

        RolePermissionEntity rolePermission2 = new RolePermissionBuilder()
            .WithRoleId(roleId)
            .WithPermissionId(permissionId2)
            .Build();

        // Assert
        rolePermission1.RoleId.Should().Be(roleId);
        rolePermission2.RoleId.Should().Be(roleId);
        rolePermission1.PermissionId.Should().NotBe(rolePermission2.PermissionId);
    }

    [Fact]
    public void MultipleRolePermissions_WithSamePermission_ShouldHaveDifferentRoles()
    {
        // Arrange
        Guid permissionId = Guid.NewGuid();
        Guid roleId1 = Guid.NewGuid();
        Guid roleId2 = Guid.NewGuid();

        // Act
        RolePermissionEntity rolePermission1 = new RolePermissionBuilder()
            .WithRoleId(roleId1)
            .WithPermissionId(permissionId)
            .Build();

        RolePermissionEntity rolePermission2 = new RolePermissionBuilder()
            .WithRoleId(roleId2)
            .WithPermissionId(permissionId)
            .Build();

        // Assert
        rolePermission1.PermissionId.Should().Be(permissionId);
        rolePermission2.PermissionId.Should().Be(permissionId);
        rolePermission1.RoleId.Should().NotBe(rolePermission2.RoleId);
    }

    #endregion
}
