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
public class UserErrors(
    ConflictErrorMessage conflict,
    ValidationErrorMessage validation,
    AuthenticationErrorMessage authentication,
    AuthorizationErrorMessage authorization
)
{
    /// <summary>
    /// Exposes the conflict message provider for use in validator extensions.
    /// </summary>
    public ConflictErrorMessage Conflict => conflict;

    /// <summary>
    /// Exposes the validation message provider for use in validator extensions.
    /// </summary>
    public ValidationErrorMessage Validation => validation;

    /// <summary>
    /// Exposes the authentication message provider for use in validator extensions.
    /// </summary>
    public AuthenticationErrorMessage Authentication => authentication;

    /// <summary>
    /// Exposes the authorization message provider for use in validator extensions.
    /// </summary>
    public AuthorizationErrorMessage Authorization => authorization;

    /// <summary>
    /// Throws when a user already exists during registration.
    /// </summary>
    public ConflictException EmailAlreadyExists(string email)
    {
        return new ConflictException(conflict.EmailAlreadyExists(email: email));
    }

    /// <summary>
    /// Throws when a username is already taken.
    /// </summary>
    public ConflictException UsernameAlreadyExists(string username)
    {
        return new ConflictException(conflict.UsernameAlreadyExists(username: username));
    }

    /// <summary>
    /// Throws when a phone number is already taken.
    /// </summary>
    public ConflictException PhoneNumberAlreadyExists(string phoneNumber)
    {
        return new ConflictException(conflict.PhoneNumberAlreadyExists(phoneNumber: phoneNumber));
    }

    /// <summary>
    /// Throws when a role already exists.
    /// </summary>
    public ConflictException RoleAlreadyExists(string roleName)
    {
        return new ConflictException(conflict.RoleAlreadyExists(name: roleName));
    }

    /// <summary>
    /// Throws when a role is already assigned to a user.
    /// </summary>
    public ConflictException RoleAlreadyAssignedToUser()
    {
        return new ConflictException(conflict.RoleAlreadyAssignedToUser());
    }

    /// <summary>
    /// Throws when a role is not found using the name.
    /// </summary>
    public NotFoundException RoleNotFoundByName(string roleName)
    {
        return new NotFoundException("Role", "name", keyValue: roleName);
    }

    /// <summary>
    /// Throws when a permission already exists.
    /// </summary>
    public ConflictException PermissionAlreadyExists(string resource, string action)
    {
        return new ConflictException(conflict.PermissionAlreadyExists(resource, action));
    }

    /// <summary>
    /// Throws when a permission is already assigned to a role.
    /// </summary>
    public ConflictException PermissionAlreadyAssignedToRole()
    {
        return new ConflictException(conflict.PermissionAlreadyAssignedToRole());
    }

    /// <summary>
    /// Throws when a role is already active.
    /// </summary>
    public ConflictException RoleAlreadyActive()
    {
        return new ConflictException(conflict.RoleAlreadyActive());
    }

    /// <summary>
    /// Throws when a role is already inactive.
    /// </summary>
    public ConflictException RoleAlreadyInactive()
    {
        return new ConflictException(conflict.RoleAlreadyInactive());
    }

    /// <summary>
    /// Throws when a role is already deleted.
    /// </summary>
    public ConflictException RoleAlreadyDeleted()
    {
        return new ConflictException(conflict.RoleAlreadyDeleted());
    }

    /// <summary>
    /// Throws when a role is not deleted and cannot be restored.
    /// </summary>
    public ConflictException RoleNotDeleted()
    {
        return new ConflictException(conflict.RoleNotDeleted());
    }

    /// <summary>
    /// Throws when a permission is already active.
    /// </summary>
    public ConflictException PermissionAlreadyActive()
    {
        return new ConflictException(conflict.PermissionAlreadyActive());
    }

    /// <summary>
    /// Throws when a permission is already inactive.
    /// </summary>
    public ConflictException PermissionAlreadyInactive()
    {
        return new ConflictException(conflict.PermissionAlreadyInactive());
    }

    /// <summary>
    /// Throws when a permission is already deleted.
    /// </summary>
    public ConflictException PermissionAlreadyDeleted()
    {
        return new ConflictException(conflict.PermissionAlreadyDeleted());
    }

    /// <summary>
    /// Throws when a permission is not deleted and cannot be restored.
    /// </summary>
    public ConflictException PermissionNotDeleted()
    {
        return new ConflictException(conflict.PermissionNotDeleted());
    }

    /// <summary>
    /// Throws when a core role cannot be modified.
    /// </summary>
    public BadRequestException CoreRoleCannotBeModified(string roleName)
    {
        return new BadRequestException(validation.CoreRoleCannotBeModified(roleName));
    }

    /// <summary>
    /// Throws when a core role cannot be deleted.
    /// </summary>
    public BadRequestException CoreRoleCannotBeDeleted(string roleName)
    {
        return new BadRequestException(validation.CoreRoleCannotBeDeleted(roleName));
    }

    /// <summary>
    /// Throws when a role is inactive and cannot be assigned.
    /// </summary>
    public BadRequestException RoleIsInactive()
    {
        return new BadRequestException(validation.RoleIsInactive());
    }

    /// <summary>
    /// Throws when a role is deleted and cannot be used.
    /// </summary>
    public BadRequestException RoleIsDeleted()
    {
        return new BadRequestException(validation.RoleIsDeleted());
    }

    /// <summary>
    /// Throws when a permission is inactive.
    /// </summary>
    public BadRequestException PermissionIsInactive()
    {
        return new BadRequestException(validation.PermissionIsInactive());
    }

    /// <summary>
    /// Throws when a permission is deleted.
    /// </summary>
    public BadRequestException PermissionIsDeleted()
    {
        return new BadRequestException(validation.PermissionIsDeleted());
    }

    /// <summary>
    /// Throws when a permission is not assigned to the role.
    /// </summary>
    public BadRequestException PermissionNotAssignedToRole()
    {
        return new BadRequestException(validation.PermissionNotAssignedToRole());
    }

    /// <summary>
    /// Throws when a role is not assigned to the user.
    /// </summary>
    public BadRequestException RoleNotAssignedToUser()
    {
        return new BadRequestException(validation.RoleNotAssignedToUser());
    }

    /// <summary>
    /// Throws when the account is inactive.
    /// </summary>
    public AccountInactiveException AccountInactive(string email)
    {
        return new AccountInactiveException(authorization.AccountInactive(email: email));
    }

    /// <summary>
    /// Throws when the account is not verified.
    /// </summary>
    public AccountNotVerifiedException AccountNotVerified(string email)
    {
        return new AccountNotVerifiedException(authorization.AccountNotVerified(email: email));
    }

    /// <summary>
    /// Throws when password is invalid.
    /// </summary>
    public AuthenticationException InvalidCredentials()
    {
        return new AuthenticationException(authentication.InvalidCredentials());
    }

    /// <summary>
    /// Throws when the email format is invalid.
    /// </summary>
    public AuthenticationException InvalidEmailFormat(string email)
    {
        return new AuthenticationException(validation.InvalidEmailFormat(email: email));
    }

    /// <summary>
    /// Throws when the password format is invalid.
    /// </summary>
    public AuthenticationException InvalidPasswordFormat()
    {
        return new AuthenticationException(validation.InvalidPasswordFormat());
    }

    /// <summary>
    /// Throws when the userName format is invalid.
    /// </summary>
    public BadRequestException InvalidUsernameFormat(string username)
    {
        return new BadRequestException(validation.InvalidUsernameFormat(userName: username));
    }

    /// <summary>
    /// Throws when permission resource is required.
    /// </summary>
    public BadRequestException PermissionResourceRequired()
    {
        return new BadRequestException(validation.PermissionResourceRequired());
    }

    /// <summary>
    /// Throws when permission action is required.
    /// </summary>
    public BadRequestException PermissionActionRequired()
    {
        return new BadRequestException(validation.PermissionActionRequired());
    }

    /// <summary>
    /// Throws when permission description is required.
    /// </summary>
    public BadRequestException PermissionDescriptionRequired()
    {
        return new BadRequestException(validation.PermissionDescriptionRequired());
    }

    /// <summary>
    /// Throws when role name is required.
    /// </summary>
    public BadRequestException RoleNameRequired()
    {
        return new BadRequestException(validation.RoleNameRequired());
    }

    /// <summary>
    /// Throws when role description is required.
    /// </summary>
    public BadRequestException RoleDescriptionRequired()
    {
        return new BadRequestException(validation.RoleDescriptionRequired());
    }

    /// <summary>
    /// Throws when a password update is attempted for a user without an email.
    /// </summary>
    public BadRequestException PasswordUpdateRequiresEmail()
    {
        return new BadRequestException(validation.PasswordUpdateRequiresEmail());
    }

    /// <summary>
    /// Throws when the user account is already verified.
    /// </summary>
    public ConflictException AccountAlreadyVerified()
    {
        return new ConflictException(validation.AccountAlreadyVerified());
    }

    /// <summary>
    /// Throws when no valid OTP is found for verification.
    /// </summary>
    public NotFoundException NoValidOtpFound()
    {
        return new NotFoundException(validation.NoValidOtpFound());
    }

    /// <summary>
    /// Throws when OTP verification code is invalid.
    /// </summary>
    public BadRequestException InvalidOtpCode()
    {
        return new BadRequestException(validation.InvalidOtpCode());
    }

    /// <summary>
    /// Throws when OTP has expired.
    /// </summary>
    public OtpExpirationException OtpExpired()
    {
        return new OtpExpirationException(validation.OtpExpired());
    }

    /// <summary>
    /// Throws when maximum OTP verification attempts are reached.
    /// </summary>
    public OtpAttemptsLimitException MaxOtpAttemptsReached()
    {
        return new OtpAttemptsLimitException(validation.MaxOtpAttemptsReached());
    }

    /// <summary>
    /// Throws when OTP has not been verified yet.
    /// </summary>
    public BadRequestException OtpNotYetVerified()
    {
        return new BadRequestException(validation.OtpNotYetVerified());
    }

    /// <summary>
    /// Throws when the user is not authenticated or user ID is invalid.
    /// </summary>
    public AuthenticationException InvalidUserAuthentication()
    {
        return new AuthenticationException(authentication.InvalidUserAuthentication());
    }

    /// <summary>
    /// Throws when the user does not have sufficient permissions for the operation.
    /// </summary>
    public AccessDeniedException InsufficientPermissions()
    {
        return new AccessDeniedException(authentication.InsufficientPermissions());
    }

    /// <summary>
    /// Throws when the new password is the same as the old password.
    /// </summary>
    public ConflictException NewPasswordSameAsOld()
    {
        return new ConflictException(validation.NewPasswordSameAsOld());
    }

    /// <summary>
    /// Throws when a password has not been configured for the account.
    /// </summary>
    /// <param name="provider">The OAuth provider name (e.g., Google, Facebook).</param>
    public BadRequestException PasswordNotConfigured(EnumAuthProvider provider)
    {
        return new BadRequestException(validation.PasswordNotConfigured(provider: provider));
    }

    /// <summary>
    /// Throws when the current password is incorrect (e.g., during password change).
    /// </summary>
    public BadRequestException IncorrectCurrentPassword()
    {
        return new BadRequestException(validation.IncorrectCurrentPassword());
    }

    /// <summary>
    /// Throws when an email address is required to set a password.
    /// </summary>
    public BadRequestException EmailRequiredToSetPassword()
    {
        return new BadRequestException(validation.EmailRequiredToSetPassword());
    }

    /// <summary>
    /// Throws when setting password is only allowed for external auth users (Google/Facebook).
    /// </summary>
    public BadRequestException PasswordOnlyForExternalAuth()
    {
        return new BadRequestException(validation.PasswordOnlyForExternalAuth());
    }
}
