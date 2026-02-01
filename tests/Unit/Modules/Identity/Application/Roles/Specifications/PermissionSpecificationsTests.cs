using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Specifications;

/// <summary>
/// Unit tests for Permission specifications.
/// </summary>
public class PermissionSpecificationsTests
{
    #region PermissionByIdSpecification Tests

    [Fact]
    public void PermissionByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        Guid permissionId = Guid.NewGuid();
        PermissionEntity permission = new PermissionBuilder().WithId(permissionId).Build();
        PermissionByIdSpecification spec = new(permissionId);

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PermissionByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().WithId(Guid.NewGuid()).Build();
        PermissionByIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PermissionByResourceAndActionSpecification Tests

    [Fact]
    public void PermissionByResourceAndActionSpecification_WithMatchingResourceAndAction_ShouldReturnTrue()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().WithResource("articles").WithAction("read").Build();
        PermissionByResourceAndActionSpecification spec = new("articles", "read");

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PermissionByResourceAndActionSpecification_WithDifferentResource_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().WithResource("articles").WithAction("read").Build();
        PermissionByResourceAndActionSpecification spec = new("users", "read");

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void PermissionByResourceAndActionSpecification_WithDifferentAction_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().WithResource("articles").WithAction("read").Build();
        PermissionByResourceAndActionSpecification spec = new("articles", "write");

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PermissionIsActiveSpecification Tests

    [Fact]
    public void PermissionIsActiveSpecification_WithActivePermission_ShouldReturnTrue()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.Activate();
        PermissionIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PermissionIsActiveSpecification_WithInactivePermission_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.Deactivate();
        PermissionIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PermissionIsDeletedSpecification Tests

    [Fact]
    public void PermissionIsDeletedSpecification_WithDeletedPermission_ShouldReturnTrue()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.SoftDelete();
        PermissionIsDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PermissionIsDeletedSpecification_WithNonDeletedPermission_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        PermissionIsDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PermissionNotDeletedSpecification Tests

    [Fact]
    public void PermissionNotDeletedSpecification_WithNonDeletedPermission_ShouldReturnTrue()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        PermissionNotDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PermissionNotDeletedSpecification_WithDeletedPermission_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.SoftDelete();
        PermissionNotDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActivePermissionSpecification Tests

    [Fact]
    public void ActivePermissionSpecification_WithActiveAndNonDeletedPermission_ShouldReturnTrue()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.Activate();
        ActivePermissionSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActivePermissionSpecification_WithInactivePermission_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.Deactivate();
        ActivePermissionSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ActivePermissionSpecification_WithDeletedPermission_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.Activate();
        permission.SoftDelete();
        ActivePermissionSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PermissionIsNotActiveSpecification Tests

    [Fact]
    public void PermissionIsNotActiveSpecification_WithInactivePermission_ShouldReturnTrue()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.Deactivate();
        PermissionIsNotActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PermissionIsNotActiveSpecification_WithActivePermission_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder().Build();
        permission.Activate();
        PermissionIsNotActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(permission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PermissionSearchSpecification Tests - ToExpression Only

    [Fact]
    public void PermissionSearchSpecification_ShouldCreateValidExpression()
    {
        // Arrange
        PermissionSearchSpecification spec = new("articles");

        // Act
        var expression = spec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void PermissionByResourceAndActionSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<PermissionEntity> permissions =
        [
            new PermissionBuilder().WithResource("articles").WithAction("read").Build(),
            new PermissionBuilder().WithResource("articles").WithAction("write").Build(),
            new PermissionBuilder().WithResource("users").WithAction("read").Build(),
        ];

        PermissionByResourceAndActionSpecification spec = new("articles", "read");

        // Act
        List<PermissionEntity> filtered = permissions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Resource.Should().Be("articles");
        filtered[0].Action.Should().Be("read");
    }

    [Fact]
    public void ActivePermissionSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        PermissionEntity activePermission = new PermissionBuilder().WithResource("articles").WithAction("read").Build();
        activePermission.Activate();

        PermissionEntity inactivePermission = new PermissionBuilder().WithResource("users").WithAction("write").Build();
        inactivePermission.Deactivate();

        PermissionEntity deletedPermission = new PermissionBuilder()
            .WithResource("comments")
            .WithAction("delete")
            .Build();
        deletedPermission.SoftDelete();

        List<PermissionEntity> permissions = [activePermission, inactivePermission, deletedPermission];
        ActivePermissionSpecification spec = new();

        // Act
        List<PermissionEntity> filtered = permissions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Resource.Should().Be("articles");
    }

    #endregion
}
