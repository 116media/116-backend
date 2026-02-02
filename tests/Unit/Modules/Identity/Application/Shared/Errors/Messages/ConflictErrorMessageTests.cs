using _116.Identity.Application.Shared.Errors.Messages;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="ConflictErrorMessage"/>.
/// </summary>
public class ConflictErrorMessageTests
{
    [Fact]
    public void EmailAlreadyExists_WithEmail_ShouldReturnFormattedMessage()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var message = ConflictErrorMessage.EmailAlreadyExists(email);

        // Assert
        message.Should().Be($"User with email '{email}' already exists");
    }

    [Fact]
    public void UsernameAlreadyExists_WithUsername_ShouldReturnFormattedMessage()
    {
        // Arrange
        var username = "testuser";

        // Act
        var message = ConflictErrorMessage.UsernameAlreadyExists(username);

        // Assert
        message.Should().Be($"Username '{username}' is already taken");
    }

    [Fact]
    public void PhoneNumberAlreadyExists_WithPhoneNumber_ShouldReturnFormattedMessage()
    {
        // Arrange
        var phoneNumber = "+1234567890";

        // Act
        var message = ConflictErrorMessage.PhoneNumberAlreadyExists(phoneNumber);

        // Assert
        message.Should().Be($"Phone number '{phoneNumber}' is already taken");
    }

    [Fact]
    public void RoleAlreadyExists_WithRoleName_ShouldReturnFormattedMessage()
    {
        // Arrange
        var name = "Admin";

        // Act
        var message = ConflictErrorMessage.RoleAlreadyExists(name);

        // Assert
        message.Should().Be($"Role '{name}' already exists");
    }

    [Fact]
    public void PermissionAlreadyExists_WithResourceAndAction_ShouldReturnFormattedMessage()
    {
        // Arrange
        var resource = "users";
        var action = "create";

        // Act
        var message = ConflictErrorMessage.PermissionAlreadyExists(resource, action);

        // Assert
        message.Should().Be($"Permission '{resource}.{action}' already exists");
    }

    [Fact]
    public void RoleAlreadyAssignedToUser_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.RoleAlreadyAssignedToUser();

        // Assert
        message.Should().Be("Role is already assigned to this user");
    }

    [Fact]
    public void PermissionAlreadyAssignedToRole_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.PermissionAlreadyAssignedToRole();

        // Assert
        message.Should().Be("Permission is already assigned to this role");
    }

    [Fact]
    public void RoleAlreadyActive_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.RoleAlreadyActive();

        // Assert
        message.Should().Be("Role is already active");
    }

    [Fact]
    public void RoleAlreadyInactive_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.RoleAlreadyInactive();

        // Assert
        message.Should().Be("Role is already inactive");
    }

    [Fact]
    public void RoleAlreadyDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.RoleAlreadyDeleted();

        // Assert
        message.Should().Be("Role is already deleted");
    }

    [Fact]
    public void RoleNotDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.RoleNotDeleted();

        // Assert
        message.Should().Be("Role is not deleted and cannot be restored");
    }

    [Fact]
    public void PermissionAlreadyActive_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.PermissionAlreadyActive();

        // Assert
        message.Should().Be("Permission is already active");
    }

    [Fact]
    public void PermissionAlreadyInactive_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.PermissionAlreadyInactive();

        // Assert
        message.Should().Be("Permission is already inactive");
    }

    [Fact]
    public void PermissionAlreadyDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.PermissionAlreadyDeleted();

        // Assert
        message.Should().Be("Permission is already deleted");
    }

    [Fact]
    public void PermissionNotDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        var message = ConflictErrorMessage.PermissionNotDeleted();

        // Assert
        message.Should().Be("Permission is not deleted and cannot be restored");
    }
}
