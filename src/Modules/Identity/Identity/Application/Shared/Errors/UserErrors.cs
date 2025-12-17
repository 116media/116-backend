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
    public static ConflictException EmailAlreadyExists(string email) =>
        new(ConflictErrorMessage.EmailAlreadyExists(email));
    /// <summary>
    /// Throws when a username is already taken.
    /// </summary>
    public static ConflictException UsernameAlreadyExists(string username) =>
        new(ConflictErrorMessage.UsernameAlreadyExists(username));
    /// <summary>
    /// Throws when a phone number is already taken.
    /// </summary>
    public static ConflictException PhoneNumberAlreadyExists(string phoneNumber) =>
        new(ConflictErrorMessage.PhoneNumberAlreadyExists(phoneNumber));
    /// <summary>
    /// Throws when a role already exists.
    /// </summary>
    public static ConflictException RoleAlreadyExists(string roleName) =>
        new(ConflictErrorMessage.RoleAlreadyExists(roleName));
    /// <summary>
    /// Throws when a role is already assigned to a user.
    /// </summary>
    public static ConflictException RoleAlreadyAssignedToUser() =>
        new(ConflictErrorMessage.RoleAlreadyAssignedToUser());
    /// <summary>
    /// Throws when a role is not found.
    /// </summary>
    public static NotFoundException RoleNotFound(int roleId) => new("Role", roleId);
    /// <summary>
    /// Throws when a role is not found using the name.
    /// </summary>
    public static NotFoundException RoleNotFoundByName(string roleName) => new("Role", "name", roleName);
    /// <summary>
    /// Throws when a permission is not found.
    /// </summary>
    public static NotFoundException PermissionNotFound(int permissionId) => new("Permission", permissionId);
    /// <summary>
    /// Throws when the account is inactive.
    /// </summary>
    public static AccountInactiveException AccountInactive(string email) =>
        new(AuthorizationErrorMessage.AccountInactive(email));
    /// <summary>
    /// Throws when the account is not verified.
    /// </summary>
    public static AccountNotVerifiedException AccountNotVerified(string email) =>
        new(AuthorizationErrorMessage.AccountNotVerified(email));
    /// <summary>
    /// Throws when the user is not logged in.
    /// </summary>
    public static UserNotLoggedInException UserNotLoggedIn(string email) =>
        new(AuthorizationErrorMessage.UserNotLoggedIn(email));
    /// <summary>
    /// Throws when password is invalid.
    /// </summary>
    public static AuthenticationException InvalidCredentials() =>
        new(AuthenticationErrorMessage.InvalidCredentials());
    /// <summary>
    /// Throws when the email format is invalid.
    /// </summary>
    public static AuthenticationException InvalidEmailFormat(string email) =>
        new(ValidationErrorMessage.InvalidEmailFormat(email));
    /// <summary>
    /// Throws when the password format is invalid.
    /// </summary>
    public static AuthenticationException InvalidPasswordFormat() =>
        new(ValidationErrorMessage.InvalidPasswordFormat());
    /// <summary>
    /// Throws when the userName format is invalid.
    /// </summary>
    public static BadRequestException InvalidUsernameFormat(string username) =>
        new(ValidationErrorMessage.InvalidUsernameFormat(username));
    /// <summary>
    /// Throws when permission resource is required.
    /// </summary>
    public static BadRequestException PermissionResourceRequired() =>
        new(ValidationErrorMessage.PermissionResourceRequired());
    /// <summary>
    /// Throws when permission action is required.
    /// </summary>
    public static BadRequestException PermissionActionRequired() =>
        new(ValidationErrorMessage.PermissionActionRequired());
    /// <summary>
    /// Throws when permission description is required.
    /// </summary>
    public static BadRequestException PermissionDescriptionRequired() =>
        new(ValidationErrorMessage.PermissionDescriptionRequired());
    /// <summary>
    /// Throws when role name is required.
    /// </summary>
    public static BadRequestException RoleNameRequired() =>
        new(ValidationErrorMessage.RoleNameRequired());
    /// <summary>
    /// Throws when role description is required.
    /// </summary>
    public static BadRequestException RoleDescriptionRequired() =>
        new(ValidationErrorMessage.RoleDescriptionRequired());
    /// <summary>
    /// Throws a generic bad request exception with a custom message.
    /// </summary>
    public static BadRequestException BadRequest(string message) =>
        new(message);
    /// <summary>
    /// Throws when the user account is already verified.
    /// </summary>
    public static ConflictException AccountAlreadyVerified() =>
        new(ValidationErrorMessage.AccountAlreadyVerified());
    /// <summary>
    /// Throws when no valid OTP is found for verification.
    /// </summary>
    public static NotFoundException NoValidOtpFound() =>
        new(ValidationErrorMessage.NoValidOtpFound());
    /// <summary>
    /// Throws when OTP verification code is invalid.
    /// </summary>
    public static BadRequestException InvalidOtpCode() =>
        new(ValidationErrorMessage.InvalidOtpCode());
    /// <summary>
    /// Throws when OTP has expired.
    /// </summary>
    public static OtpExpirationException OtpExpired() =>
        new(ValidationErrorMessage.OtpExpired());
    /// <summary>
    /// Throws when maximum OTP verification attempts are reached.
    /// </summary>
    public static OtpAttemptsLimitException MaxOtpAttemptsReached() =>
        new(ValidationErrorMessage.MaxOtpAttemptsReached());
    /// <summary>
    /// Throws when OTP has not been verified yet.
    /// </summary>
    public static BadRequestException OtpNotYetVerified() =>
        new(ValidationErrorMessage.OtpNotYetVerified());
    /// <summary>
    /// Throws when the user is not authenticated or user ID is invalid.
    /// </summary>
    public static AuthenticationException InvalidUserAuthentication() =>
        new(AuthenticationErrorMessage.InvalidUserAuthentication());
    /// <summary>
    /// Throws when the user does not have sufficient permissions for the operation.
    /// </summary>
    public static AccessDeniedException InsufficientPermissions() =>
        new(AuthenticationErrorMessage.InsufficientPermissions());
    /// <summary>
    /// Throws when a user is not found.
    /// </summary>
    public static NotFoundException UserNotFound() => new("User");
    /// <summary>
    /// Throws when the new password is the same as the old password.
    /// </summary>
    public static ConflictException NewPasswordSameAsOld() =>
        new(ValidationErrorMessage.NewPasswordSameAsOld());
    /// <summary>
    /// Throws when a password has not been configured for the account.
    /// </summary>
    /// <param name="provider">The OAuth provider name (e.g., Google, Facebook).</param>
    public static BadRequestException PasswordNotConfigured(EnumAuthProvider provider) =>
        new(ValidationErrorMessage.PasswordNotConfigured(provider));
    /// <summary>
    /// Throws when the current password is incorrect (e.g., during password change).
    /// </summary>
    public static BadRequestException IncorrectCurrentPassword() =>
        new(ValidationErrorMessage.IncorrectCurrentPassword());
    /// <summary>
    /// Throws when an email address is required to set a password.
    /// </summary>
    public static BadRequestException EmailRequiredToSetPassword() =>
        new(ValidationErrorMessage.EmailRequiredToSetPassword());
    /// <summary>
    /// Throws when setting password is only allowed for external auth users (Google/Facebook).
    /// </summary>
    public static BadRequestException PasswordOnlyForExternalAuth() =>
        new(ValidationErrorMessage.PasswordOnlyForExternalAuth());
}
