using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="RoleEntity"/>.
/// </summary>
public class RoleEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateRole()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = TestConstants.Role.ValidName;
        string description = TestConstants.Role.ValidDescription;

        // Act
        RoleEntity role = RoleEntity.Create(id, name, description);

        // Assert
        role.Id.Should().Be(id);
        role.Name.Should().Be(name);
        role.Description.Should().Be(description);
        role.IsActive.Should().BeTrue();
        role.IsDeleted.Should().BeFalse();
        role.DeletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string description = TestConstants.Role.ValidDescription;

        // Act
        Action act = () => RoleEntity.Create(id, invalidName!, description);

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
        string name = TestConstants.Role.ValidName;

        // Act
        Action act = () => RoleEntity.Create(id, name, invalidDescription!);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateRole()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();
        string newName = "UpdatedRole";
        string newDescription = "Updated role description for testing purposes.";

        // Act
        role.Update(newName, newDescription);

        // Assert
        role.Name.Should().Be(newName);
        role.Description.Should().Be(newDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();

        // Act
        Action act = () => role.Update(invalidName!, TestConstants.Role.ValidDescription);

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
        RoleEntity role = new RoleBuilder().Build();

        // Act
        Action act = () => role.Update(TestConstants.Role.ValidName, invalidDescription!);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldActivateAndReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().AsInactive().Build();

        // Act
        bool result = role.Activate();

        // Assert
        result.Should().BeTrue();
        role.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build(); // Default is active

        // Act
        bool result = role.Activate();

        // Assert
        result.Should().BeFalse();
        role.IsActive.Should().BeTrue();
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateAndReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build(); // Default is active

        // Act
        bool result = role.Deactivate();

        // Assert
        result.Should().BeTrue();
        role.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().AsInactive().Build();

        // Act
        bool result = role.Deactivate();

        // Assert
        result.Should().BeFalse();
        role.IsActive.Should().BeFalse();
    }

    #endregion

    #region SoftDelete Tests

    [Fact]
    public void SoftDelete_WhenNotDeleted_ShouldSoftDeleteAndReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();

        // Act
        bool result = role.SoftDelete();

        // Assert
        result.Should().BeTrue();
        role.IsDeleted.Should().BeTrue();
        role.IsActive.Should().BeFalse();
        role.DeletedAt.Should().NotBeNull();
        role.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().AsDeleted().Build();

        // Act
        bool result = role.SoftDelete();

        // Assert
        result.Should().BeFalse();
        role.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Restore Tests

    [Fact]
    public void Restore_WhenDeleted_ShouldRestoreAndReturnTrue()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().AsDeleted().Build();

        // Act
        bool result = role.Restore();

        // Assert
        result.Should().BeTrue();
        role.IsDeleted.Should().BeFalse();
        role.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenNotDeleted_ShouldReturnFalse()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().Build();

        // Act
        bool result = role.Restore();

        // Assert
        result.Should().BeFalse();
        role.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Restore_ShouldNotAutomaticallyActivate()
    {
        // Arrange
        RoleEntity role = new RoleBuilder().AsDeleted().Build();

        // Act
        role.Restore();

        // Assert
        role.IsDeleted.Should().BeFalse();
        role.IsActive.Should().BeFalse(); // Was deactivated by SoftDelete, should remain inactive
    }

    #endregion
}
