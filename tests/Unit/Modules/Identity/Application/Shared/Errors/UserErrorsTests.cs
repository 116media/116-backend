using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Messages;
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
    private readonly UserErrors _errors = TestErrorsFactory.CreateUserErrors();
    private readonly ConflictErrorMessage _conflict = LocalizerFactory.CreateMessage<ConflictErrorMessage>();
    private readonly ValidationErrorMessage _validation = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
    private readonly AuthenticationErrorMessage _authentication =
        LocalizerFactory.CreateMessage<AuthenticationErrorMessage>();
    private readonly AuthorizationErrorMessage _authorization =
        LocalizerFactory.CreateMessage<AuthorizationErrorMessage>();

    [Fact]
    public void Validation_ShouldReturnValidationErrorMessage()
    {
        ValidationErrorMessage result = _errors.Validation;

        result.Should().NotBeNull();
    }

    [Fact]
    public void EmailAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        const string email = "user@example.com";

        // Act
        ConflictException exception = _errors.EmailAlreadyExists(email);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.EmailAlreadyExists(email));
    }

    [Fact]
    public void UsernameAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        const string username = "john_doe";

        // Act
        ConflictException exception = _errors.UsernameAlreadyExists(username);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.UsernameAlreadyExists(username));
    }

    [Fact]
    public void PhoneNumberAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        const string phone = "+1234567890";

        // Act
        ConflictException exception = _errors.PhoneNumberAlreadyExists(phone);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.PhoneNumberAlreadyExists(phone));
    }

    [Fact]
    public void RoleAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        const string roleName = "Admin";

        // Act
        ConflictException exception = _errors.RoleAlreadyExists(roleName);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.RoleAlreadyExists(roleName));
    }

    [Fact]
    public void RoleAlreadyAssignedToUser_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.RoleAlreadyAssignedToUser();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.RoleAlreadyAssignedToUser());
    }

    [Fact]
    public void RoleNotFoundByName_ShouldReturnNotFoundException()
    {
        // Arrange
        const string roleName = "SuperAdmin";

        // Act
        NotFoundException exception = _errors.RoleNotFoundByName(roleName);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("Role");
        exception.Message.Should().Contain(roleName);
    }

    [Fact]
    public void PermissionAlreadyExists_ShouldReturnConflictException()
    {
        // Arrange
        const string resource = "users";
        const string action = "read";

        // Act
        ConflictException exception = _errors.PermissionAlreadyExists(resource, action);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.PermissionAlreadyExists(resource, action));
    }

    [Fact]
    public void PermissionAlreadyAssignedToRole_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.PermissionAlreadyAssignedToRole();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.PermissionAlreadyAssignedToRole());
    }

    [Fact]
    public void RoleAlreadyActive_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.RoleAlreadyActive();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.RoleAlreadyActive());
    }

    [Fact]
    public void RoleAlreadyInactive_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.RoleAlreadyInactive();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.RoleAlreadyInactive());
    }

    [Fact]
    public void RoleAlreadyDeleted_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.RoleAlreadyDeleted();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.RoleAlreadyDeleted());
    }

    [Fact]
    public void RoleNotDeleted_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.RoleNotDeleted();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.RoleNotDeleted());
    }

    [Fact]
    public void PermissionAlreadyActive_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.PermissionAlreadyActive();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.PermissionAlreadyActive());
    }

    [Fact]
    public void PermissionAlreadyInactive_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.PermissionAlreadyInactive();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.PermissionAlreadyInactive());
    }

    [Fact]
    public void PermissionAlreadyDeleted_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.PermissionAlreadyDeleted();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.PermissionAlreadyDeleted());
    }

    [Fact]
    public void PermissionNotDeleted_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.PermissionNotDeleted();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.PermissionNotDeleted());
    }

    [Fact]
    public void AccountAlreadyVerified_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.AccountAlreadyVerified();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_validation.AccountAlreadyVerified());
    }

    [Fact]
    public void NewPasswordSameAsOld_ShouldReturnConflictException()
    {
        ConflictException exception = _errors.NewPasswordSameAsOld();

        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_validation.NewPasswordSameAsOld());
    }

    [Fact]
    public void CoreRoleCannotBeDeleted_ShouldReturnBadRequestException()
    {
        // Arrange
        const string roleName = "Visitor";

        // Act
        BadRequestException exception = _errors.CoreRoleCannotBeDeleted(roleName);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.CoreRoleCannotBeDeleted(roleName));
    }

    [Fact]
    public void RoleIsInactive_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.RoleIsInactive();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.RoleIsInactive());
    }

    [Fact]
    public void RoleIsDeleted_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.RoleIsDeleted();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.RoleIsDeleted());
    }

    [Fact]
    public void PermissionIsInactive_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PermissionIsInactive();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PermissionIsInactive());
    }

    [Fact]
    public void PermissionIsDeleted_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PermissionIsDeleted();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PermissionIsDeleted());
    }

    [Fact]
    public void PermissionNotAssignedToRole_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PermissionNotAssignedToRole();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PermissionNotAssignedToRole());
    }

    [Fact]
    public void RoleNotAssignedToUser_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.RoleNotAssignedToUser();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.RoleNotAssignedToUser());
    }

    [Fact]
    public void InvalidUsernameFormat_ShouldReturnBadRequestException()
    {
        // Arrange
        const string username = "bad username!";

        // Act
        BadRequestException exception = _errors.InvalidUsernameFormat(username);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.InvalidUsernameFormat(username));
    }

    [Fact]
    public void PermissionResourceRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PermissionResourceRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PermissionResourceRequired());
    }

    [Fact]
    public void PermissionActionRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PermissionActionRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PermissionActionRequired());
    }

    [Fact]
    public void PermissionDescriptionRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PermissionDescriptionRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PermissionDescriptionRequired());
    }

    [Fact]
    public void RoleNameRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.RoleNameRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.RoleNameRequired());
    }

    [Fact]
    public void RoleDescriptionRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.RoleDescriptionRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.RoleDescriptionRequired());
    }

    [Fact]
    public void InvalidOtpCode_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.InvalidOtpCode();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.InvalidOtpCode());
    }

    [Fact]
    public void OtpNotYetVerified_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.OtpNotYetVerified();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.OtpNotYetVerified());
    }

    [Fact]
    public void PasswordNotConfigured_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PasswordNotConfigured(EnumAuthProvider.Google);

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PasswordNotConfigured(EnumAuthProvider.Google));
    }

    [Fact]
    public void IncorrectCurrentPassword_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.IncorrectCurrentPassword();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.IncorrectCurrentPassword());
    }

    [Fact]
    public void EmailRequiredToSetPassword_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.EmailRequiredToSetPassword();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.EmailRequiredToSetPassword());
    }

    [Fact]
    public void PasswordOnlyForExternalAuth_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.PasswordOnlyForExternalAuth();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_validation.PasswordOnlyForExternalAuth());
    }

    [Fact]
    public void AccountInactive_ShouldReturnAccountInactiveException()
    {
        // Arrange
        const string email = "user@example.com";

        // Act
        AccountInactiveException exception = _errors.AccountInactive(email);

        // Assert
        exception.Should().BeOfType<AccountInactiveException>();
        exception.Message.Should().Be(_authorization.AccountInactive(email));
    }

    [Fact]
    public void AccountNotVerified_ShouldReturnAccountNotVerifiedException()
    {
        // Arrange
        const string email = "user@example.com";

        // Act
        AccountNotVerifiedException exception = _errors.AccountNotVerified(email);

        // Assert
        exception.Should().BeOfType<AccountNotVerifiedException>();
        exception.Message.Should().Be(_authorization.AccountNotVerified(email));
    }

    [Fact]
    public void InvalidCredentials_ShouldReturnAuthenticationException()
    {
        AuthenticationException exception = _errors.InvalidCredentials();

        exception.Should().BeOfType<AuthenticationException>();
        exception.Message.Should().Be(_authentication.InvalidCredentials());
    }

    [Fact]
    public void InvalidEmailFormat_ShouldReturnAuthenticationException()
    {
        // Arrange
        const string email = "not-an-email";

        // Act
        AuthenticationException exception = _errors.InvalidEmailFormat(email);

        // Assert
        exception.Should().BeOfType<AuthenticationException>();
        exception.Message.Should().Be(_validation.InvalidEmailFormat(email));
    }

    [Fact]
    public void InvalidPasswordFormat_ShouldReturnAuthenticationException()
    {
        AuthenticationException exception = _errors.InvalidPasswordFormat();

        exception.Should().BeOfType<AuthenticationException>();
        exception.Message.Should().Be(_validation.InvalidPasswordFormat());
    }

    [Fact]
    public void InvalidUserAuthentication_ShouldReturnAuthenticationException()
    {
        AuthenticationException exception = _errors.InvalidUserAuthentication();

        exception.Should().BeOfType<AuthenticationException>();
        exception.Message.Should().Be(_authentication.InvalidUserAuthentication());
    }

    [Fact]
    public void InsufficientPermissions_ShouldReturnAccessDeniedException()
    {
        AccessDeniedException exception = _errors.InsufficientPermissions();

        exception.Should().BeOfType<AccessDeniedException>();
        exception.Message.Should().Be(_authentication.InsufficientPermissions());
    }

    [Fact]
    public void NoValidOtpFound_ShouldReturnNotFoundException()
    {
        NotFoundException exception = _errors.NoValidOtpFound();

        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Be(_validation.NoValidOtpFound());
    }

    [Fact]
    public void OtpExpired_ShouldReturnOtpExpirationException()
    {
        OtpExpirationException exception = _errors.OtpExpired();

        exception.Should().BeOfType<OtpExpirationException>();
        exception.Message.Should().Be(_validation.OtpExpired());
    }

    [Fact]
    public void MaxOtpAttemptsReached_ShouldReturnOtpAttemptsLimitException()
    {
        OtpAttemptsLimitException exception = _errors.MaxOtpAttemptsReached();

        exception.Should().BeOfType<OtpAttemptsLimitException>();
        exception.Message.Should().Be(_validation.MaxOtpAttemptsReached());
    }

    [Fact]
    public void ConflictErrorMessage_Localizer_EmailAlreadyExists_ShouldReturnLocalizedString()
    {
        _conflict.Localizer["EmailAlreadyExists"].Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AuthenticationErrorMessage_Localizer_InvalidCredentials_ShouldReturnLocalizedString()
    {
        _authentication.Localizer["InvalidCredentials"].Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AuthorizationErrorMessage_Localizer_AccountInactive_ShouldReturnLocalizedString()
    {
        _authorization.Localizer["AccountInactive"].Value.Should().NotBeNullOrEmpty();
    }
}
