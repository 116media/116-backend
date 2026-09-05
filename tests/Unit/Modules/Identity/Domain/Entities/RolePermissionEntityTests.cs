using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Factories.Identity;
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
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        var rolePermission = RolePermissionEntity.Create(roleId, permissionId);

        // Assert
        rolePermission.RoleId.Should().Be(roleId);
        rolePermission.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public void Create_ShouldLeaveTheKeyUnsetForTheStoreGenerator()
    {
        // Act
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        rolePermission.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldNotSetNavigationProperties()
    {
        // Arrange & Act
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), Guid.NewGuid());

        // Assert — navigations stay null until EF loads the related entities
        rolePermission.Role.Should().BeNull();
        rolePermission.Permission.Should().BeNull();
    }

    #endregion

    #region Builder Tests

    [Fact]
    public void Builder_ShouldCreateRolePermissionWithSpecifiedValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity rolePermission = new RolePermissionBuilder()
            .WithId(id)
            .ForRoleAndPermission(roleId, permissionId)
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
        RolePermissionEntity rolePermission = RolePermissionFactory.Create();

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
        var roleId = Guid.NewGuid();
        var permissionId1 = Guid.NewGuid();
        var permissionId2 = Guid.NewGuid();

        // Act
        RolePermissionEntity rolePermission1 = RolePermissionFactory.Create(roleId, permissionId1);

        RolePermissionEntity rolePermission2 = RolePermissionFactory.Create(roleId, permissionId2);

        // Assert
        rolePermission1.RoleId.Should().Be(roleId);
        rolePermission2.RoleId.Should().Be(roleId);
        rolePermission1.PermissionId.Should().NotBe(rolePermission2.PermissionId);
    }

    [Fact]
    public void MultipleRolePermissions_WithSamePermission_ShouldHaveDifferentRoles()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();

        // Act
        RolePermissionEntity rolePermission1 = RolePermissionFactory.Create(roleId1, permissionId);

        RolePermissionEntity rolePermission2 = RolePermissionFactory.Create(roleId2, permissionId);

        // Assert
        rolePermission1.PermissionId.Should().Be(permissionId);
        rolePermission2.PermissionId.Should().Be(permissionId);
        rolePermission1.RoleId.Should().NotBe(rolePermission2.RoleId);
    }

    #endregion
}
