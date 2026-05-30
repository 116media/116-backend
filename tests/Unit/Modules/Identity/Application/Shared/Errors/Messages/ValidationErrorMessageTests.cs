using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="ValidationErrorMessage"/>.
/// </summary>
public class ValidationErrorMessageTests
{
    private readonly ValidationErrorMessage _message = LocalizerFactory.CreateMessage<ValidationErrorMessage>("en");

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void InvalidEmailFormat_WithEmail_ShouldReturnFormattedMessage()
    {
        // Arrange
        string email = "invalid-email";

        // Act
        string message = _message.InvalidEmailFormat(email);

        // Assert
        message.Should().Be($"Invalid email format: {email}.");
    }

    [Fact]
    public void InvalidPasswordFormat_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.InvalidPasswordFormat();

        // Assert
        message.Should().Be("Password does not meet security requirements.");
    }

    [Fact]
    public void InvalidUsernameFormat_WithUsername_ShouldReturnFormattedMessage()
    {
        // Arrange
        string userName = "bad@user";

        // Act
        string message = _message.InvalidUsernameFormat(userName);

        // Assert
        message.Should().Be($"Username '{userName}' does not meet requirements.");
    }

    [Fact]
    public void PermissionResourceRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionResourceRequired();

        // Assert
        message.Should().Be("Permission resource is required.");
    }

    [Fact]
    public void PermissionActionRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionActionRequired();

        // Assert
        message.Should().Be("Permission action is required.");
    }

    [Fact]
    public void PermissionDescriptionRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionDescriptionRequired();

        // Assert
        message.Should().Be("Permission description is required.");
    }

    [Fact]
    public void RoleNameRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleNameRequired();

        // Assert
        message.Should().Be(TestConstants.ValidationMessages.Role.NameRequired);
    }

    [Fact]
    public void RoleDescriptionRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleDescriptionRequired();

        // Assert
        message.Should().Be(TestConstants.ValidationMessages.Role.DescriptionRequired);
    }

    [Fact]
    public void AccountAlreadyVerified_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.AccountAlreadyVerified();

        // Assert
        message.Should().Be("Account is already verified.");
    }

    [Fact]
    public void NoValidOtpFound_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.NoValidOtpFound();

        // Assert
        message.Should().Be("No valid verification code found. Please request a new verification code.");
    }

    [Fact]
    public void InvalidOtpCode_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.InvalidOtpCode();

        // Assert
        message.Should().Be("Invalid verification code. Please check and try again.");
    }

    [Fact]
    public void OtpExpired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.OtpExpired();

        // Assert
        message.Should().Be("Verification code has expired. Please request a new verification code.");
    }

    [Fact]
    public void MaxOtpAttemptsReached_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.MaxOtpAttemptsReached();

        // Assert
        message.Should().Be("Maximum verification attempts reached. Please request a new verification code.");
    }

    [Fact]
    public void OtpNotYetVerified_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.OtpNotYetVerified();

        // Assert
        message.Should().Be("Please complete the verification step before proceeding.");
    }

    [Fact]
    public void NewPasswordSameAsOld_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.NewPasswordSameAsOld();

        // Assert
        message.Should().Be("New password must be different from your current password.");
    }

    [Fact]
    public void PasswordNotConfigured_WithProvider_ShouldReturnFormattedMessage()
    {
        // Arrange
        var provider = EnumAuthProvider.Google;

        // Act
        string message = _message.PasswordNotConfigured(provider);

        // Assert
        message.Should().Be($"This account was created via {provider}. Please set a password before changing it.");
    }

    [Fact]
    public void IncorrectCurrentPassword_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.IncorrectCurrentPassword();

        // Assert
        message.Should().Be("The current password you entered is incorrect.");
    }

    [Fact]
    public void EmailRequiredToSetPassword_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.EmailRequiredToSetPassword();

        // Assert
        message.Should().Be("An email address is required to set a password.");
    }

    [Fact]
    public void PasswordOnlyForExternalAuth_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PasswordOnlyForExternalAuth();

        // Assert
        message.Should().Be("Setting password is only allowed for Google or Facebook users.");
    }

    [Fact]
    public void CoreRoleCannotBeModified_WithRoleName_ShouldReturnFormattedMessage()
    {
        // Arrange
        string roleName = "SuperAdmin";

        // Act
        string message = _message.CoreRoleCannotBeModified(roleName);

        // Assert
        message.Should().Be($"Core role '{roleName}' cannot be modified.");
    }

    [Fact]
    public void CoreRoleCannotBeDeleted_WithRoleName_ShouldReturnFormattedMessage()
    {
        // Arrange
        string roleName = "Admin";

        // Act
        string message = _message.CoreRoleCannotBeDeleted(roleName);

        // Assert
        message.Should().Be($"Core role '{roleName}' cannot be deleted.");
    }

    [Fact]
    public void RoleIsInactive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleIsInactive();

        // Assert
        message.Should().Be("Cannot assign an inactive role to a user.");
    }

    [Fact]
    public void RoleIsDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleIsDeleted();

        // Assert
        message.Should().Be("Cannot use a deleted role.");
    }

    [Fact]
    public void PermissionIsInactive_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionIsInactive();

        // Assert
        message.Should().Be("Cannot assign an inactive permission to a role.");
    }

    [Fact]
    public void PermissionIsDeleted_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionIsDeleted();

        // Assert
        message.Should().Be("Cannot use a deleted permission.");
    }

    [Fact]
    public void PermissionNotAssignedToRole_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.PermissionNotAssignedToRole();

        // Assert
        message.Should().Be("Permission is not assigned to this role.");
    }

    [Fact]
    public void RoleNotAssignedToUser_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.RoleNotAssignedToUser();

        // Assert
        message.Should().Be("Role is not assigned to this user.");
    }
}
