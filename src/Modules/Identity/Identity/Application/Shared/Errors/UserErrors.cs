using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Exceptions;

namespace _116.Identity.Application.Shared.Errors;

/// <summary>
/// User domain error factory providing simple, readable exception creation.
/// Usage: UserErrors.UserAlreadyExists(email) or UserErrors.UserNotFound(userId)
/// </summary>
public static class UserErrors
{
    /// <summary>
    /// Throws when a user already exists during registration.
    /// </summary>
    public static ConflictException EmailAlreadyExists(string email)
    {
        return new ConflictException(ConflictErrorMessage.EmailAlreadyExists(email: email));
    }

    /// <summary>
    /// Throws when a username is already taken.
    /// </summary>
    public static ConflictException UsernameAlreadyExists(string username)
    {
        return new ConflictException(ConflictErrorMessage.UsernameAlreadyExists(username: username));
    }

    /// <summary>
    /// Throws when a phone number is already taken.
    /// </summary>
    public static ConflictException PhoneNumberAlreadyExists(string phoneNumber)
    {
        return new ConflictException(ConflictErrorMessage.PhoneNumberAlreadyExists(phoneNumber: phoneNumber));
    }

    /// <summary>
    /// Throws when a role already exists.
    /// </summary>
    public static ConflictException RoleAlreadyExists(string roleName)
    {
        return new ConflictException(ConflictErrorMessage.RoleAlreadyExists(name: roleName));
    }

    /// <summary>
    /// Throws when a role is already assigned to a user.
    /// </summary>
    public static ConflictException RoleAlreadyAssignedToUser()
    {
        return new ConflictException(ConflictErrorMessage.RoleAlreadyAssignedToUser());
    }

    /// <summary>
    /// Throws when a role is not found.
    /// </summary>
    public static NotFoundException RoleNotFound(int roleId)
    {
        return new NotFoundException("Role", key: roleId);
    }

    /// <summary>
    /// Throws when a role is not found using the name.
    /// </summary>
    public static NotFoundException RoleNotFoundByName(string roleName)
    {
        return new NotFoundException("Role", "name", keyValue: roleName);
    }

    /// <summary>
    /// Throws when a permission is not found.
    /// </summary>
    public static NotFoundException PermissionNotFound(int permissionId)
    {
        return new NotFoundException("Permission", key: permissionId);
    }

    /// <summary>
    /// Throws when the account is inactive.
    /// </summary>
    public static AccountInactiveException AccountInactive(string email)
    {
        return new AccountInactiveException(AuthorizationErrorMessage.AccountInactive(email: email));
    }

    /// <summary>
    /// Throws when the account is not verified.
    /// </summary>
    public static AccountNotVerifiedException AccountNotVerified(string email)
    {
        return new AccountNotVerifiedException(AuthorizationErrorMessage.AccountNotVerified(email: email));
    }

    /// <summary>
    /// Throws when password is invalid.
    /// </summary>
    public static AuthenticationException InvalidCredentials()
    {
        return new AuthenticationException(AuthenticationErrorMessage.InvalidCredentials());
    }

    /// <summary>
    /// Throws when the email format is invalid.
    /// </summary>
    public static AuthenticationException InvalidEmailFormat(string email)
    {
        return new AuthenticationException(ValidationErrorMessage.InvalidEmailFormat(email: email));
    }

    /// <summary>
    /// Throws when the password format is invalid.
    /// </summary>
    public static AuthenticationException InvalidPasswordFormat()
    {
        return new AuthenticationException(ValidationErrorMessage.InvalidPasswordFormat());
    }

    /// <summary>
    /// Throws when the userName format is invalid.
    /// </summary>
    public static BadRequestException InvalidUsernameFormat(string username)
    {
        return new BadRequestException(ValidationErrorMessage.InvalidUsernameFormat(userName: username));
    }

    /// <summary>
    /// Throws when permission resource is required.
    /// </summary>
    public static BadRequestException PermissionResourceRequired()
    {
        return new BadRequestException(ValidationErrorMessage.PermissionResourceRequired());
    }

    /// <summary>
    /// Throws when permission action is required.
    /// </summary>
    public static BadRequestException PermissionActionRequired()
    {
        return new BadRequestException(ValidationErrorMessage.PermissionActionRequired());
    }

    /// <summary>
    /// Throws when permission description is required.
    /// </summary>
    public static BadRequestException PermissionDescriptionRequired()
    {
        return new BadRequestException(ValidationErrorMessage.PermissionDescriptionRequired());
    }

    /// <summary>
    /// Throws when role name is required.
    /// </summary>
    public static BadRequestException RoleNameRequired()
    {
        return new BadRequestException(ValidationErrorMessage.RoleNameRequired());
    }

    /// <summary>
    /// Throws when role description is required.
    /// </summary>
    public static BadRequestException RoleDescriptionRequired()
    {
        return new BadRequestException(ValidationErrorMessage.RoleDescriptionRequired());
    }

    /// <summary>
    /// Throws a generic bad request exception with a custom message.
    /// </summary>
    public static BadRequestException BadRequest(string message)
    {
        return new BadRequestException(message: message);
    }

    /// <summary>
    /// Throws when the user account is already verified.
    /// </summary>
    public static ConflictException AccountAlreadyVerified()
    {
        return new ConflictException(ValidationErrorMessage.AccountAlreadyVerified());
    }

    /// <summary>
    /// Throws when no valid OTP is found for verification.
    /// </summary>
    public static NotFoundException NoValidOtpFound()
    {
        return new NotFoundException(ValidationErrorMessage.NoValidOtpFound());
    }

    /// <summary>
    /// Throws when OTP verification code is invalid.
    /// </summary>
    public static BadRequestException InvalidOtpCode()
    {
        return new BadRequestException(ValidationErrorMessage.InvalidOtpCode());
    }

    /// <summary>
    /// Throws when OTP has expired.
    /// </summary>
    public static OtpExpirationException OtpExpired()
    {
        return new OtpExpirationException(ValidationErrorMessage.OtpExpired());
    }

    /// <summary>
    /// Throws when maximum OTP verification attempts are reached.
    /// </summary>
    public static OtpAttemptsLimitException MaxOtpAttemptsReached()
    {
        return new OtpAttemptsLimitException(ValidationErrorMessage.MaxOtpAttemptsReached());
    }

    /// <summary>
    /// Throws when OTP has not been verified yet.
    /// </summary>
    public static BadRequestException OtpNotYetVerified()
    {
        return new BadRequestException(ValidationErrorMessage.OtpNotYetVerified());
    }

    /// <summary>
    /// Throws when the user is not authenticated or user ID is invalid.
    /// </summary>
    public static AuthenticationException InvalidUserAuthentication()
    {
        return new AuthenticationException(AuthenticationErrorMessage.InvalidUserAuthentication());
    }

    /// <summary>
    /// Throws when the user does not have sufficient permissions for the operation.
    /// </summary>
    public static AccessDeniedException InsufficientPermissions()
    {
        return new AccessDeniedException(AuthenticationErrorMessage.InsufficientPermissions());
    }

    /// <summary>
    /// Throws when a user is not found.
    /// </summary>
    public static NotFoundException UserNotFound()
    {
        return new NotFoundException("User");
    }

    /// <summary>
    /// Throws when the new password is the same as the old password.
    /// </summary>
    public static ConflictException NewPasswordSameAsOld()
    {
        return new ConflictException(ValidationErrorMessage.NewPasswordSameAsOld());
    }

    /// <summary>
    /// Throws when a password has not been configured for the account.
    /// </summary>
    /// <param name="provider">The OAuth provider name (e.g., Google, Facebook).</param>
    public static BadRequestException PasswordNotConfigured(EnumAuthProvider provider)
    {
        return new BadRequestException(ValidationErrorMessage.PasswordNotConfigured(provider: provider));
    }

    /// <summary>
    /// Throws when the current password is incorrect (e.g., during password change).
    /// </summary>
    public static BadRequestException IncorrectCurrentPassword()
    {
        return new BadRequestException(ValidationErrorMessage.IncorrectCurrentPassword());
    }

    /// <summary>
    /// Throws when an email address is required to set a password.
    /// </summary>
    public static BadRequestException EmailRequiredToSetPassword()
    {
        return new BadRequestException(ValidationErrorMessage.EmailRequiredToSetPassword());
    }

    /// <summary>
    /// Throws when setting password is only allowed for external auth users (Google/Facebook).
    /// </summary>
    public static BadRequestException PasswordOnlyForExternalAuth()
    {
        return new BadRequestException(ValidationErrorMessage.PasswordOnlyForExternalAuth());
    }
}
