using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="ConflictErrorMessage"/>.
/// </summary>
public class ConflictErrorMessageTests
{
    private readonly ConflictErrorMessage _message = LocalizerFactory.CreateMessage<ConflictErrorMessage>("en");

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void EmailAlreadyExists_WithEmail_ShouldReturnFormattedMessage()
    {
        // Arrange
        string email = "test@example.com";

        // Act
        string message = _message.EmailAlreadyExists(email);

        // Assert
        message.Should().Be($"User with email '{email}' already exists.");
    }

    [Fact]
    public void UsernameAlreadyExists_WithUsername_ShouldReturnFormattedMessage()
    {
        // Arrange
        string username = "testuser";

        // Act
        string message = _message.UsernameAlreadyExists(username);

        // Assert
        message.Should().Be($"Username '{username}' is already taken.");
    }

    [Fact]
    public void PhoneNumberAlreadyExists_WithPhoneNumber_ShouldReturnFormattedMessage()
    {
        // Arrange
        string phoneNumber = "+1234567890";

        // Act
        string message = _message.PhoneNumberAlreadyExists(phoneNumber);

        // Assert
        message.Should().Be($"Phone number '{phoneNumber}' is already taken.");
    }

    [Fact]
    public void RoleAlreadyExists_WithRoleName_ShouldReturnFormattedMessage()
    {
        // Arrange
        string name = "Admin";

        // Act
        string message = _message.RoleAlreadyExists(name);

        // Assert
        message.Should().Be($"Role '{name}' already exists.");
    }

    [Fact]
    public void PermissionAlreadyExists_WithResourceAndAction_ShouldReturnFormattedMessage()
    {
        // Arrange
        string resource = "users";
        string action = "create";

        // Act
        string message = _message.PermissionAlreadyExists(resource, action);

        // Assert
        message.Should().Be($"Permission '{resource}.{action}' already exists.");
    }

    [Fact]
    public void RoleAlreadyAssignedToUser_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleAlreadyAssignedToUser();

        // Assert
        message.Should().Be("Role is already assigned to this user.");
    }

    [Fact]
    public void PermissionAlreadyAssignedToRole_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionAlreadyAssignedToRole();

        // Assert
        message.Should().Be("Permission is already assigned to this role.");
    }

    [Fact]
    public void RoleAlreadyActive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleAlreadyActive();

        // Assert
        message.Should().Be("Role is already active.");
    }

    [Fact]
    public void RoleAlreadyInactive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleAlreadyInactive();

        // Assert
        message.Should().Be("Role is already inactive.");
    }

    [Fact]
    public void RoleAlreadyDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleAlreadyDeleted();

        // Assert
        message.Should().Be("Role is already deleted.");
    }

    [Fact]
    public void RoleNotDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleNotDeleted();

        // Assert
        message.Should().Be("Role is not deleted and cannot be restored.");
    }

    [Fact]
    public void PermissionAlreadyActive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionAlreadyActive();

        // Assert
        message.Should().Be("Permission is already active.");
    }

    [Fact]
    public void PermissionAlreadyInactive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionAlreadyInactive();

        // Assert
        message.Should().Be("Permission is already inactive.");
    }

    [Fact]
    public void PermissionAlreadyDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionAlreadyDeleted();

        // Assert
        message.Should().Be("Permission is already deleted.");
    }

    [Fact]
    public void PermissionNotDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionNotDeleted();

        // Assert
        message.Should().Be("Permission is not deleted and cannot be restored.");
    }
}
