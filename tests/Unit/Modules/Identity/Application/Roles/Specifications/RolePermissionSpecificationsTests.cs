using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Specifications;

/// <summary>
/// Unit tests for RolePermission specifications.
/// </summary>
public class RolePermissionSpecificationsTests
{
    #region RolePermissionByRoleIdSpecification Tests

    [Fact]
    public void RolePermissionByRoleIdSpecification_WithMatchingRoleId_ShouldReturnTrue()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        RolePermissionEntity rolePermission = new RolePermissionBuilder().WithRoleId(roleId).Build();
        RolePermissionByRoleIdSpecification spec = new(roleId);

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RolePermissionByRoleIdSpecification_WithDifferentRoleId_ShouldReturnFalse()
    {
        // Arrange
        RolePermissionEntity rolePermission = new RolePermissionBuilder().WithRoleId(Guid.NewGuid()).Build();
        RolePermissionByRoleIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RolePermissionByPermissionIdSpecification Tests

    [Fact]
    public void RolePermissionByPermissionIdSpecification_WithMatchingPermissionId_ShouldReturnTrue()
    {
        // Arrange
        Guid permissionId = Guid.NewGuid();
        RolePermissionEntity rolePermission = new RolePermissionBuilder().WithPermissionId(permissionId).Build();
        RolePermissionByPermissionIdSpecification spec = new(permissionId);

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RolePermissionByPermissionIdSpecification_WithDifferentPermissionId_ShouldReturnFalse()
    {
        // Arrange
        RolePermissionEntity rolePermission = new RolePermissionBuilder().WithPermissionId(Guid.NewGuid()).Build();
        RolePermissionByPermissionIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RolePermissionByIdSpecification Tests

    [Fact]
    public void RolePermissionByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        RolePermissionEntity rolePermission = RolePermissionFactory.CreateWithId(id);
        RolePermissionByIdSpecification spec = new(id);

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RolePermissionByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        RolePermissionEntity rolePermission = RolePermissionFactory.CreateWithId(Guid.NewGuid());
        RolePermissionByIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RolePermissionByRoleAndPermissionSpecification Tests

    [Fact]
    public void RolePermissionByRoleAndPermissionSpecification_WithMatchingBoth_ShouldReturnTrue()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();
        RolePermissionEntity rolePermission = RolePermissionFactory.Create(roleId, permissionId);
        RolePermissionByRoleAndPermissionSpecification spec = new(roleId, permissionId);

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RolePermissionByRoleAndPermissionSpecification_WithDifferentRoleId_ShouldReturnFalse()
    {
        // Arrange
        Guid permissionId = Guid.NewGuid();
        RolePermissionEntity rolePermission = new RolePermissionBuilder()
            .WithRoleId(Guid.NewGuid())
            .WithPermissionId(permissionId)
            .Build();
        RolePermissionByRoleAndPermissionSpecification spec = new(Guid.NewGuid(), permissionId);

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RolePermissionByRoleAndPermissionSpecification_WithDifferentPermissionId_ShouldReturnFalse()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        RolePermissionEntity rolePermission = new RolePermissionBuilder()
            .WithRoleId(roleId)
            .WithPermissionId(Guid.NewGuid())
            .Build();
        RolePermissionByRoleAndPermissionSpecification spec = new(roleId, Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RolePermissionByRoleAndPermissionSpecification_WithBothDifferent_ShouldReturnFalse()
    {
        // Arrange
        RolePermissionEntity rolePermission = new RolePermissionBuilder()
            .WithRoleId(Guid.NewGuid())
            .WithPermissionId(Guid.NewGuid())
            .Build();
        RolePermissionByRoleAndPermissionSpecification spec = new(Guid.NewGuid(), Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(rolePermission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void RolePermissionByRoleIdSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid targetRoleId = Guid.NewGuid();
        List<RolePermissionEntity> rolePermissions =
        [
            new RolePermissionBuilder().WithRoleId(targetRoleId).Build(),
            new RolePermissionBuilder().WithRoleId(targetRoleId).Build(),
            new RolePermissionBuilder().WithRoleId(Guid.NewGuid()).Build(),
        ];

        RolePermissionByRoleIdSpecification spec = new(targetRoleId);

        // Act
        List<RolePermissionEntity> filtered = rolePermissions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(rp => rp.RoleId == targetRoleId);
    }

    [Fact]
    public void RolePermissionByRoleAndPermissionSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        List<RolePermissionEntity> rolePermissions =
        [
            RolePermissionFactory.Create(roleId, permissionId),
            RolePermissionFactory.Create(roleId, Guid.NewGuid()),
            RolePermissionFactory.Create(Guid.NewGuid(), permissionId),
        ];

        RolePermissionByRoleAndPermissionSpecification spec = new(roleId, permissionId);

        // Act
        List<RolePermissionEntity> filtered = rolePermissions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].RoleId.Should().Be(roleId);
        filtered[0].PermissionId.Should().Be(permissionId);
    }

    #endregion
}
