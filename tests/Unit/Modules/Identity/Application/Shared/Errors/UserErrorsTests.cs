using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="UserErrors"/>.
/// </summary>
public class UserErrorsTests
{
    [Fact]
    public void EmailAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var exception = UserErrors.EmailAlreadyExists(email);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void UsernameAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        var username = "testuser";

        // Act
        var exception = UserErrors.UsernameAlreadyExists(username);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(username);
    }

    [Fact]
    public void PhoneNumberAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        var phoneNumber = "+1234567890";

        // Act
        var exception = UserErrors.PhoneNumberAlreadyExists(phoneNumber);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(phoneNumber);
    }

    [Fact]
    public void RoleAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        var roleName = "Admin";

        // Act
        var exception = UserErrors.RoleAlreadyExists(roleName);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void RoleAlreadyAssignedToUser_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.RoleAlreadyAssignedToUser();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleNotFoundByName_ShouldReturnNotFoundException()
    {
        // Arrange
        var roleName = "NonExistent";

        // Act
        var exception = UserErrors.RoleNotFoundByName(roleName);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void PermissionAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        var resource = "users";
        var action = "create";

        // Act
        var exception = UserErrors.PermissionAlreadyExists(resource, action);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain($"{resource}.{action}");
    }

    [Fact]
    public void PermissionAlreadyAssignedToRole_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.PermissionAlreadyAssignedToRole();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleAlreadyActive_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.RoleAlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleAlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.RoleAlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleAlreadyDeleted_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.RoleAlreadyDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleNotDeleted_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.RoleNotDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionAlreadyActive_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.PermissionAlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionAlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.PermissionAlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionAlreadyDeleted_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.PermissionAlreadyDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionNotDeleted_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.PermissionNotDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void CoreRoleCannotBeModified_ShouldReturnBadRequestException()
    {
        // Arrange
        var roleName = "SuperAdmin";

        // Act
        var exception = UserErrors.CoreRoleCannotBeModified(roleName);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void CoreRoleCannotBeDeleted_ShouldReturnBadRequestException()
    {
        // Arrange
        var roleName = "Admin";

        // Act
        var exception = UserErrors.CoreRoleCannotBeDeleted(roleName);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void RoleIsInactive_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.RoleIsInactive();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleIsDeleted_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.RoleIsDeleted();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionIsInactive_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.PermissionIsInactive();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionIsDeleted_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.PermissionIsDeleted();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionNotAssignedToRole_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.PermissionNotAssignedToRole();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleNotAssignedToUser_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.RoleNotAssignedToUser();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void AccountInactive_ShouldReturnAccountInactiveException()
    {
        // Arrange
        var email = "inactive@example.com";

        // Act
        var exception = UserErrors.AccountInactive(email);

        // Assert
        exception.Should().BeOfType<AccountInactiveException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void AccountNotVerified_ShouldReturnAccountNotVerifiedException()
    {
        // Arrange
        var email = "unverified@example.com";

        // Act
        var exception = UserErrors.AccountNotVerified(email);

        // Assert
        exception.Should().BeOfType<AccountNotVerifiedException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void InvalidCredentials_ShouldReturnAuthenticationException()
    {
        // Act
        var exception = UserErrors.InvalidCredentials();

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
    }

    [Fact]
    public void InvalidEmailFormat_ShouldReturnAuthenticationException()
    {
        // Arrange
        var email = "invalid-email";

        // Act
        var exception = UserErrors.InvalidEmailFormat(email);

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void InvalidPasswordFormat_ShouldReturnAuthenticationException()
    {
        // Act
        var exception = UserErrors.InvalidPasswordFormat();

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
    }

    [Fact]
    public void InvalidUsernameFormat_ShouldReturnBadRequestException()
    {
        // Arrange
        var username = "bad@user";

        // Act
        var exception = UserErrors.InvalidUsernameFormat(username);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(username);
    }

    [Fact]
    public void PermissionResourceRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.PermissionResourceRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionActionRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.PermissionActionRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionDescriptionRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.PermissionDescriptionRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.RoleNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleDescriptionRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.RoleDescriptionRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void BadRequest_WithMessage_ShouldReturnBadRequestException()
    {
        // Arrange
        var message = "Custom error";

        // Act
        var exception = UserErrors.BadRequest(message);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void AccountAlreadyVerified_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.AccountAlreadyVerified();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void NoValidOtpFound_ShouldReturnNotFoundException()
    {
        // Act
        var exception = UserErrors.NoValidOtpFound();

        // Assert
        exception.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void InvalidOtpCode_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.InvalidOtpCode();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void OtpExpired_ShouldReturnOtpExpirationException()
    {
        // Act
        var exception = UserErrors.OtpExpired();

        // Assert
        exception.Should().BeOfType<OtpExpirationException>();
    }

    [Fact]
    public void MaxOtpAttemptsReached_ShouldReturnOtpAttemptsLimitException()
    {
        // Act
        var exception = UserErrors.MaxOtpAttemptsReached();

        // Assert
        exception.Should().BeOfType<OtpAttemptsLimitException>();
    }

    [Fact]
    public void OtpNotYetVerified_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.OtpNotYetVerified();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void InvalidUserAuthentication_ShouldReturnAuthenticationException()
    {
        // Act
        var exception = UserErrors.InvalidUserAuthentication();

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
    }

    [Fact]
    public void InsufficientPermissions_ShouldReturnAccessDeniedException()
    {
        // Act
        var exception = UserErrors.InsufficientPermissions();

        // Assert
        exception.Should().BeOfType<AccessDeniedException>();
    }

    [Fact]
    public void NewPasswordSameAsOld_ShouldReturnConflictException()
    {
        // Act
        var exception = UserErrors.NewPasswordSameAsOld();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PasswordNotConfigured_ShouldReturnBadRequestException()
    {
        // Arrange
        var provider = EnumAuthProvider.Google;

        // Act
        var exception = UserErrors.PasswordNotConfigured(provider);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(provider.ToString());
    }

    [Fact]
    public void IncorrectCurrentPassword_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.IncorrectCurrentPassword();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void EmailRequiredToSetPassword_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.EmailRequiredToSetPassword();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PasswordOnlyForExternalAuth_ShouldReturnBadRequestException()
    {
        // Act
        var exception = UserErrors.PasswordOnlyForExternalAuth();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }
}
