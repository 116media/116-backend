using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Facade;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="UserErrors"/>.
/// </summary>
public class UserErrorsTests
{
    private readonly UserErrors _userErrors = TestErrorsFactory.CreateUserErrors();

    [Fact]
    public void EmailAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        string email = "test@example.com";

        // Act
        ConflictException exception = _userErrors.EmailAlreadyExists(email);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void UsernameAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        string username = "testuser";

        // Act
        ConflictException exception = _userErrors.UsernameAlreadyExists(username);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(username);
    }

    [Fact]
    public void PhoneNumberAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        string phoneNumber = "+1234567890";

        // Act
        ConflictException exception = _userErrors.PhoneNumberAlreadyExists(phoneNumber);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(phoneNumber);
    }

    [Fact]
    public void RoleAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        string roleName = "Admin";

        // Act
        ConflictException exception = _userErrors.RoleAlreadyExists(roleName);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void RoleAlreadyAssignedToUser_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.RoleAlreadyAssignedToUser();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleNotFoundByName_ShouldReturnNotFoundException()
    {
        // Arrange
        string roleName = "NonExistent";

        // Act
        NotFoundException exception = _userErrors.RoleNotFoundByName(roleName);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void PermissionAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        string resource = "users";
        string action = "create";

        // Act
        ConflictException exception = _userErrors.PermissionAlreadyExists(resource, action);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain($"{resource}.{action}");
    }

    [Fact]
    public void PermissionAlreadyAssignedToRole_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.PermissionAlreadyAssignedToRole();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleAlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.RoleAlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleAlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.RoleAlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleAlreadyDeleted_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.RoleAlreadyDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void RoleNotDeleted_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.RoleNotDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionAlreadyActive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.PermissionAlreadyActive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionAlreadyInactive_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.PermissionAlreadyInactive();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionAlreadyDeleted_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.PermissionAlreadyDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PermissionNotDeleted_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.PermissionNotDeleted();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void CoreRoleCannotBeModified_ShouldReturnBadRequestException()
    {
        // Arrange
        string roleName = "SuperAdmin";

        // Act
        BadRequestException exception = _userErrors.CoreRoleCannotBeModified(roleName);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void CoreRoleCannotBeDeleted_ShouldReturnBadRequestException()
    {
        // Arrange
        string roleName = "Admin";

        // Act
        BadRequestException exception = _userErrors.CoreRoleCannotBeDeleted(roleName);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void RoleIsInactive_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.RoleIsInactive();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleIsDeleted_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.RoleIsDeleted();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionIsInactive_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.PermissionIsInactive();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionIsDeleted_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.PermissionIsDeleted();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionNotAssignedToRole_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.PermissionNotAssignedToRole();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleNotAssignedToUser_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.RoleNotAssignedToUser();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void AccountInactive_ShouldReturnAccountInactiveException()
    {
        // Arrange
        string email = "inactive@example.com";

        // Act
        AccountInactiveException exception = _userErrors.AccountInactive(email);

        // Assert
        exception.Should().BeOfType<AccountInactiveException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void AccountNotVerified_ShouldReturnAccountNotVerifiedException()
    {
        // Arrange
        string email = "unverified@example.com";

        // Act
        AccountNotVerifiedException exception = _userErrors.AccountNotVerified(email);

        // Assert
        exception.Should().BeOfType<AccountNotVerifiedException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void InvalidCredentials_ShouldReturnAuthenticationException()
    {
        // Act
        AuthenticationException exception = _userErrors.InvalidCredentials();

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
    }

    [Fact]
    public void InvalidEmailFormat_ShouldReturnAuthenticationException()
    {
        // Arrange
        string email = "invalid-email";

        // Act
        AuthenticationException exception = _userErrors.InvalidEmailFormat(email);

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
        exception.Message.Should().Contain(email);
    }

    [Fact]
    public void InvalidPasswordFormat_ShouldReturnAuthenticationException()
    {
        // Act
        AuthenticationException exception = _userErrors.InvalidPasswordFormat();

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
    }

    [Fact]
    public void InvalidUsernameFormat_ShouldReturnBadRequestException()
    {
        // Arrange
        string username = "bad@user";

        // Act
        BadRequestException exception = _userErrors.InvalidUsernameFormat(username);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(username);
    }

    [Fact]
    public void PermissionResourceRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.PermissionResourceRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionActionRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.PermissionActionRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PermissionDescriptionRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.PermissionDescriptionRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.RoleNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void RoleDescriptionRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.RoleDescriptionRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void BadRequest_WithMessage_ShouldReturnBadRequestException()
    {
        // Arrange
        string message = "Custom error";

        // Act
        BadRequestException exception = _userErrors.BadRequest(message);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void AccountAlreadyVerified_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.AccountAlreadyVerified();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void NoValidOtpFound_ShouldReturnNotFoundException()
    {
        // Act
        NotFoundException exception = _userErrors.NoValidOtpFound();

        // Assert
        exception.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public void InvalidOtpCode_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.InvalidOtpCode();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void OtpExpired_ShouldReturnOtpExpirationException()
    {
        // Act
        OtpExpirationException exception = _userErrors.OtpExpired();

        // Assert
        exception.Should().BeOfType<OtpExpirationException>();
    }

    [Fact]
    public void MaxOtpAttemptsReached_ShouldReturnOtpAttemptsLimitException()
    {
        // Act
        OtpAttemptsLimitException exception = _userErrors.MaxOtpAttemptsReached();

        // Assert
        exception.Should().BeOfType<OtpAttemptsLimitException>();
    }

    [Fact]
    public void OtpNotYetVerified_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.OtpNotYetVerified();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void InvalidUserAuthentication_ShouldReturnAuthenticationException()
    {
        // Act
        AuthenticationException exception = _userErrors.InvalidUserAuthentication();

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
    }

    [Fact]
    public void InsufficientPermissions_ShouldReturnAccessDeniedException()
    {
        // Act
        AccessDeniedException exception = _userErrors.InsufficientPermissions();

        // Assert
        exception.Should().BeOfType<AccessDeniedException>();
    }

    [Fact]
    public void NewPasswordSameAsOld_ShouldReturnConflictException()
    {
        // Act
        ConflictException exception = _userErrors.NewPasswordSameAsOld();

        // Assert
        exception.Should().BeOfType<ConflictException>();
    }

    [Fact]
    public void PasswordNotConfigured_ShouldReturnBadRequestException()
    {
        // Arrange
        var provider = EnumAuthProvider.Google;

        // Act
        BadRequestException exception = _userErrors.PasswordNotConfigured(provider);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(provider.ToString());
    }

    [Fact]
    public void IncorrectCurrentPassword_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.IncorrectCurrentPassword();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void EmailRequiredToSetPassword_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.EmailRequiredToSetPassword();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }

    [Fact]
    public void PasswordOnlyForExternalAuth_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _userErrors.PasswordOnlyForExternalAuth();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
    }
}
