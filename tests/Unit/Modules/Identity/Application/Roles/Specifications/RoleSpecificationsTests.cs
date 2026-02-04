using _116.Identity.Application.Roles.Specifications;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Specifications;

/// <summary>
/// Unit tests for Role specifications.
/// </summary>
public class RoleSpecificationsTests
{
    #region RoleByNameSpecification Tests

    [Fact]
    public void RoleByNameSpecification_WithMatchingName_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().WithName("Admin").Build();
        RoleByNameSpecification spec = new("Admin");

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RoleByNameSpecification_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().WithName("Admin").Build();
        RoleByNameSpecification spec = new("Visitor");

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RoleByNameSpecification_IsCaseSensitive()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().WithName("Admin").Build();
        RoleByNameSpecification spec = new("admin");

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RoleByIdSpecification Tests

    [Fact]
    public void RoleByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        RoleEntity role = new RoleBuilder().WithId(roleId).Build();
        RoleByIdSpecification spec = new(roleId);

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RoleByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().WithId(Guid.NewGuid()).Build();
        RoleByIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RoleIsActiveSpecification Tests

    [Fact]
    public void RoleIsActiveSpecification_WithActiveRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Activate();
        RoleIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RoleIsActiveSpecification_WithInactiveRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Deactivate();
        RoleIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RoleIsDeletedSpecification Tests

    [Fact]
    public void RoleIsDeletedSpecification_WithDeletedRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.SoftDelete();
        RoleIsDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RoleIsDeletedSpecification_WithNonDeletedRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        RoleIsDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RoleNotDeletedSpecification Tests

    [Fact]
    public void RoleNotDeletedSpecification_WithNonDeletedRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        RoleNotDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RoleNotDeletedSpecification_WithDeletedRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.SoftDelete();
        RoleNotDeletedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActiveRoleSpecification Tests

    [Fact]
    public void ActiveRoleSpecification_WithActiveAndNonDeletedRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Activate();
        ActiveRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActiveRoleSpecification_WithInactiveRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Deactivate();
        ActiveRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ActiveRoleSpecification_WithDeletedRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Activate();
        role.SoftDelete();
        ActiveRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ActiveRoleSpecification_WithInactiveAndDeletedRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Deactivate();
        role.SoftDelete();
        ActiveRoleSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RoleIsNotActiveSpecification Tests

    [Fact]
    public void RoleIsNotActiveSpecification_WithInactiveRole_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Deactivate();
        RoleIsNotActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RoleIsNotActiveSpecification_WithActiveRole_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        role.Activate();
        RoleIsNotActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(role);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RoleSearchSpecification Tests - ToExpression Only

    [Fact]
    public void RoleSearchSpecification_ShouldCreateValidExpression()
    {
        // Arrange
        RoleSearchSpecification spec = new("Admin");

        // Act
        var expression = spec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Compile().Should().NotBeNull();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void RoleByNameSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        List<RoleEntity> roles =
        [
            new RoleBuilder().WithName("Admin").Build(),
            new RoleBuilder().WithName("Visitor").Build(),
            new RoleBuilder().WithName("Moderator").Build(),
        ];

        RoleByNameSpecification spec = new("Admin");

        // Act
        List<RoleEntity> filtered = roles.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Name.Should().Be("Admin");
    }

    [Fact]
    public void ActiveRoleSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        RoleEntity activeRole = new RoleBuilder().WithName("Active").Build();
        activeRole.Activate();

        RoleEntity inactiveRole = new RoleBuilder().WithName("Inactive").Build();
        inactiveRole.Deactivate();

        RoleEntity deletedRole = new RoleBuilder().WithName("Deleted").Build();
        deletedRole.SoftDelete();

        List<RoleEntity> roles = [activeRole, inactiveRole, deletedRole];
        ActiveRoleSpecification spec = new();

        // Act
        List<RoleEntity> filtered = roles.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].Name.Should().Be("Active");
    }

    #endregion
}
