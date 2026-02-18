using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="PermissionEntity"/>.
/// </summary>
public class PermissionEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreatePermission()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string resource = TestConstants.Permission.ValidResource;
        string action = TestConstants.Permission.ValidAction;
        string description = TestConstants.Permission.ValidDescription;

        // Act
        PermissionEntity permission = PermissionEntity.Create(id, resource, action, description);

        // Assert
        permission.Id.Should().Be(id);
        permission.Resource.Should().Be(resource);
        permission.Action.Should().Be(action);
        permission.Description.Should().Be(description);
        permission.IsActive.Should().BeTrue();
        permission.IsDeleted.Should().BeFalse();
        permission.DeletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidResource_ShouldThrowBadRequestException(string? invalidResource)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            PermissionEntity.Create(
                id,
                invalidResource!,
                TestConstants.Permission.ValidAction,
                TestConstants.Permission.ValidDescription
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidAction_ShouldThrowBadRequestException(string? invalidAction)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            PermissionEntity.Create(
                id,
                TestConstants.Permission.ValidResource,
                invalidAction!,
                TestConstants.Permission.ValidDescription
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ShouldThrowBadRequestException(string? invalidDescription)
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Action act = () =>
            PermissionEntity.Create(
                id,
                TestConstants.Permission.ValidResource,
                TestConstants.Permission.ValidAction,
                invalidDescription!
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParameters_ShouldUpdatePermission()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();
        string newResource = "articles";
        string newAction = "create";
        string newDescription = "Updated permission description for testing.";

        // Act
        permission.Update(newResource, newAction, newDescription);

        // Assert
        permission.Resource.Should().Be(newResource);
        permission.Action.Should().Be(newAction);
        permission.Description.Should().Be(newDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidResource_ShouldThrowBadRequestException(string? invalidResource)
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();

        // Act
        Action act = () =>
            permission.Update(
                invalidResource!,
                TestConstants.Permission.ValidAction,
                TestConstants.Permission.ValidDescription
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidAction_ShouldThrowBadRequestException(string? invalidAction)
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();

        // Act
        Action act = () =>
            permission.Update(
                TestConstants.Permission.ValidResource,
                invalidAction!,
                TestConstants.Permission.ValidDescription
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidDescription_ShouldThrowBadRequestException(string? invalidDescription)
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();

        // Act
        Action act = () =>
            permission.Update(
                TestConstants.Permission.ValidResource,
                TestConstants.Permission.ValidAction,
                invalidDescription!
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldActivateAndReturnTrue()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateInactive();

        // Act
        bool result = permission.Activate();

        // Assert
        result.Should().BeTrue();
        permission.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(); // Default is active

        // Act
        bool result = permission.Activate();

        // Assert
        result.Should().BeFalse();
        permission.IsActive.Should().BeTrue();
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateAndReturnTrue()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(); // Default is active

        // Act
        bool result = permission.Deactivate();

        // Assert
        result.Should().BeTrue();
        permission.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateInactive();

        // Act
        bool result = permission.Deactivate();

        // Assert
        result.Should().BeFalse();
        permission.IsActive.Should().BeFalse();
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_WhenNotDeleted_ShouldSoftDeleteAndReturnTrue()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();

        // Act
        bool result = permission.SoftDelete();

        // Assert
        result.Should().BeTrue();
        permission.IsDeleted.Should().BeTrue();
        permission.IsActive.Should().BeFalse();
        permission.DeletedAt.Should().NotBeNull();
        permission.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDeleted();

        // Act
        bool result = permission.SoftDelete();

        // Assert
        result.Should().BeFalse();
        permission.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Restore Tests

    [Fact]
    public void Restore_WhenDeleted_ShouldRestoreAndReturnTrue()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDeleted();

        // Act
        bool result = permission.Restore();

        // Assert
        result.Should().BeTrue();
        permission.IsDeleted.Should().BeFalse();
        permission.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenNotDeleted_ShouldReturnFalse()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();

        // Act
        bool result = permission.Restore();

        // Assert
        result.Should().BeFalse();
        permission.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Restore_ShouldNotAutomaticallyActivate()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDeleted();

        // Act
        permission.Restore();

        // Assert
        permission.IsDeleted.Should().BeFalse();
        permission.IsActive.Should().BeFalse(); // Was deactivated by SoftDelete, should remain inactive
    }

    #endregion
}
