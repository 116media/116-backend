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
        string email = "test@example.com";

        // Act
        string message = ConflictErrorMessage.EmailAlreadyExists(email);

        // Assert
        message.Should().Be($"User with email '{email}' already exists");
    }

    [Fact]
    public void UsernameAlreadyExists_WithUsername_ShouldReturnFormattedMessage()
    {
        // Arrange
        string username = "testuser";

        // Act
        string message = ConflictErrorMessage.UsernameAlreadyExists(username);

        // Assert
        message.Should().Be($"Username '{username}' is already taken");
    }

    [Fact]
    public void PhoneNumberAlreadyExists_WithPhoneNumber_ShouldReturnFormattedMessage()
    {
        // Arrange
        string phoneNumber = "+1234567890";

        // Act
        string message = ConflictErrorMessage.PhoneNumberAlreadyExists(phoneNumber);

        // Assert
        message.Should().Be($"Phone number '{phoneNumber}' is already taken");
    }

    [Fact]
    public void RoleAlreadyExists_WithRoleName_ShouldReturnFormattedMessage()
    {
        // Arrange
        string name = "Admin";

        // Act
        string message = ConflictErrorMessage.RoleAlreadyExists(name);

        // Assert
        message.Should().Be($"Role '{name}' already exists");
    }

    [Fact]
    public void PermissionAlreadyExists_WithResourceAndAction_ShouldReturnFormattedMessage()
    {
        // Arrange
        string resource = "users";
        string action = "create";

        // Act
        string message = ConflictErrorMessage.PermissionAlreadyExists(resource, action);

        // Assert
        message.Should().Be($"Permission '{resource}.{action}' already exists");
    }

    [Fact]
    public void RoleAlreadyAssignedToUser_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.RoleAlreadyAssignedToUser();

        // Assert
        message.Should().Be("Role is already assigned to this user");
    }

    [Fact]
    public void PermissionAlreadyAssignedToRole_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.PermissionAlreadyAssignedToRole();

        // Assert
        message.Should().Be("Permission is already assigned to this role");
    }

    [Fact]
    public void RoleAlreadyActive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.RoleAlreadyActive();

        // Assert
        message.Should().Be("Role is already active");
    }

    [Fact]
    public void RoleAlreadyInactive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.RoleAlreadyInactive();

        // Assert
        message.Should().Be("Role is already inactive");
    }

    [Fact]
    public void RoleAlreadyDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.RoleAlreadyDeleted();

        // Assert
        message.Should().Be("Role is already deleted");
    }

    [Fact]
    public void RoleNotDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.RoleNotDeleted();

        // Assert
        message.Should().Be("Role is not deleted and cannot be restored");
    }

    [Fact]
    public void PermissionAlreadyActive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.PermissionAlreadyActive();

        // Assert
        message.Should().Be("Permission is already active");
    }

    [Fact]
    public void PermissionAlreadyInactive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.PermissionAlreadyInactive();

        // Assert
        message.Should().Be("Permission is already inactive");
    }

    [Fact]
    public void PermissionAlreadyDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.PermissionAlreadyDeleted();

        // Assert
        message.Should().Be("Permission is already deleted");
    }

    [Fact]
    public void PermissionNotDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = ConflictErrorMessage.PermissionNotDeleted();

        // Assert
        message.Should().Be("Permission is not deleted and cannot be restored");
    }
}
