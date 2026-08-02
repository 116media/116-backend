using _116.Identity.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace _116.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Provides validation-related error messages for the <c>User</c> domain.
/// These messages describe failures due to invalid input or format requirements.
/// </summary>
public class ValidationErrorMessage(IStringLocalizer<ValidationErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Error message indicating that an email address is required.
    /// </summary>
    public string EmailRequired() => localizer["EmailRequired"];

    /// <summary>
    /// Error message indicating that an email address exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string EmailTooLong(int max) => string.Format(localizer["EmailTooLong"], max);

    /// <summary>
    /// Error message indicating that an email address has an invalid format (validator-level).
    /// </summary>
    public string InvalidEmailFormatMsg() => localizer["InvalidEmailFormatSimple"];

    /// <summary>
    /// Error message indicating that a password is required.
    /// </summary>
    public string PasswordRequired() => localizer["PasswordRequired"];

    /// <summary>
    /// Gets the localized display name for the new password field.
    /// Used as the field label in password strength error messages.
    /// </summary>
    public string NewPasswordFieldName() => localizer["NewPasswordFieldName"];

    /// <summary>
    /// Error message indicating that a password is too short.
    /// </summary>
    /// <param name="fieldName">The display name of the field.</param>
    /// <param name="min">The minimum required length.</param>
    public string PasswordTooShort(string fieldName, int min) =>
        string.Format(localizer["PasswordTooShort"], fieldName, min);

    /// <summary>
    /// Error message indicating that a password does not meet complexity rules.
    /// </summary>
    /// <param name="fieldName">The display name of the field.</param>
    public string PasswordComplexity(string fieldName) => string.Format(localizer["PasswordComplexity"], fieldName);

    /// <summary>
    /// Error message indicating that the current password is required.
    /// </summary>
    public string CurrentPasswordRequired() => localizer["CurrentPasswordRequired"];

    /// <summary>
    /// Error message indicating that a username is required.
    /// </summary>
    public string UsernameRequired() => localizer["UsernameRequired"];

    /// <summary>
    /// Error message indicating that a username is too short.
    /// </summary>
    /// <param name="min">The minimum required length.</param>
    public string UsernameTooShort(int min) => string.Format(localizer["UsernameTooShort"], min);

    /// <summary>
    /// Error message indicating that a username exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string UsernameTooLong(int max) => string.Format(localizer["UsernameTooLong"], max);

    /// <summary>
    /// Error message indicating that a username contains invalid characters.
    /// </summary>
    public string UsernameInvalidChars() => localizer["UsernameInvalidChars"];

    /// <summary>
    /// Error message indicating that email or username is required for login.
    /// </summary>
    public string EmailOrUsernameRequired() => localizer["EmailOrUsernameRequired"];

    /// <summary>
    /// Error message indicating that a verification code is required.
    /// </summary>
    public string OtpCodeRequired() => localizer["OtpCodeRequired"];

    /// <summary>
    /// Error message indicating that a verification code has the wrong length.
    /// </summary>
    /// <param name="length">The required exact length.</param>
    public string OtpCodeWrongLength(int length) => string.Format(localizer["OtpCodeWrongLength"], length);

    /// <summary>
    /// Error message indicating that a verification code must contain only numbers.
    /// </summary>
    public string OtpCodeNotNumeric() => localizer["OtpCodeNotNumeric"];

    /// <summary>
    /// Error message indicating that an OTP purpose is required.
    /// </summary>
    public string OtpPurposeRequired() => localizer["OtpPurposeRequired"];

    /// <summary>
    /// Error message indicating that an OTP purpose value is invalid.
    /// </summary>
    public string OtpPurposeInvalid() => localizer["OtpPurposeInvalid"];

    /// <summary>
    /// Error message indicating that the country name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string CountryNameTooLong(int max) => string.Format(localizer["CountryNameTooLong"], max);

    /// <summary>
    /// Error message indicating that the country ISO code exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string CountryIsoCodeTooLong(int max) => string.Format(localizer["CountryIsoCodeTooLong"], max);

    /// <summary>
    /// Error message indicating that the country ISO code is invalid.
    /// </summary>
    public string CountryIsoCodeInvalid() => localizer["CountryIsoCodeInvalid"];

    /// <summary>
    /// Error message indicating that the country dial code exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string CountryDialCodeTooLong(int max) => string.Format(localizer["CountryDialCodeTooLong"], max);

    /// <summary>
    /// Error message indicating that the country dial code is invalid.
    /// </summary>
    /// <param name="max">The maximum number of digits after the + sign.</param>
    public string CountryDialCodeInvalid(int max) => string.Format(localizer["CountryDialCodeInvalid"], max);

    /// <summary>
    /// Error message indicating that the partial phone number exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string PartialPhoneNumberTooLong(int max) => string.Format(localizer["PartialPhoneNumberTooLong"], max);

    /// <summary>
    /// Error message indicating that an avatar file is required.
    /// </summary>
    public string AvatarFileRequired() => localizer["AvatarFileRequired"];

    /// <summary>
    /// Error message indicating that the avatar file exceeds the size limit.
    /// </summary>
    /// <param name="maxMb">The maximum file size in megabytes.</param>
    public string AvatarFileTooLarge(long maxMb) => string.Format(localizer["AvatarFileTooLarge"], maxMb);

    /// <summary>
    /// Error message indicating that the avatar file type is not allowed.
    /// </summary>
    /// <param name="allowed">Comma-separated list of allowed MIME types.</param>
    public string AvatarFileInvalidType(string allowed) => string.Format(localizer["AvatarFileInvalidType"], allowed);

    /// <summary>
    /// Error message indicating that the avatar file extension is not allowed.
    /// </summary>
    /// <param name="allowed">Comma-separated list of allowed extensions.</param>
    public string AvatarFileInvalidExtension(string allowed) =>
        string.Format(localizer["AvatarFileInvalidExtension"], allowed);

    /// <summary>
    /// Error message indicating that the avatar URL is not a valid HTTP/HTTPS URL.
    /// </summary>
    public string AvatarUrlInvalid() => localizer["AvatarUrlInvalid"];

    /// <summary>
    /// Error message indicating that the auth provider is required.
    /// </summary>
    public string AuthProviderRequired() => localizer["AuthProviderRequired"];

    /// <summary>
    /// Error message indicating that the auth provider is invalid.
    /// </summary>
    public string AuthProviderInvalid() => localizer["AuthProviderInvalid"];

    /// <summary>
    /// Error message indicating that a refresh token is required.
    /// </summary>
    public string RefreshTokenRequired() => localizer["RefreshTokenRequired"];

    /// <summary>
    /// Error message indicating that a permission resource name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string PermissionResourceTooLong(int max) => string.Format(localizer["PermissionResourceTooLong"], max);

    /// <summary>
    /// Error message indicating that a permission action exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string PermissionActionTooLong(int max) => string.Format(localizer["PermissionActionTooLong"], max);

    /// <summary>
    /// Error message indicating that a permission description exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string PermissionDescriptionTooLong(int max) =>
        string.Format(localizer["PermissionDescriptionTooLong"], max);

    /// <summary>
    /// Error message indicating that a role name exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string RoleNameTooLong(int max) => string.Format(localizer["RoleNameTooLong"], max);

    /// <summary>
    /// Error message indicating that a role description exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string RoleDescriptionTooLong(int max) => string.Format(localizer["RoleDescriptionTooLong"], max);

    /// <summary>
    /// Gets an error message for when an email has an invalid format.
    /// </summary>
    /// <param name="email">The invalid email address.</param>
    /// <returns>
    /// A formatted error message indicating that the provided email format is invalid.
    /// </returns>
    public string InvalidEmailFormat(string email)
    {
        return string.Format(localizer["InvalidEmailFormat"], email);
    }

    /// <summary>
    /// Error message indicating that a password does not meet security requirements.
    /// </summary>
    public string InvalidPasswordFormat()
    {
        return localizer["InvalidPasswordFormat"];
    }

    /// <summary>
    /// Gets an error message for when a username does not meet required rules or format.
    /// </summary>
    /// <param name="userName">The invalid username.</param>
    /// <returns>
    /// A formatted error message indicating that the specified username is invalid.
    /// </returns>
    public string InvalidUsernameFormat(string userName)
    {
        return string.Format(localizer["InvalidUsernameFormat"], userName);
    }

    /// <summary>
    /// Error message indicating that permission resource is required.
    /// </summary>
    public string PermissionResourceRequired()
    {
        return localizer["PermissionResourceRequired"];
    }

    /// <summary>
    /// Error message indicating that permission action is required.
    /// </summary>
    public string PermissionActionRequired()
    {
        return localizer["PermissionActionRequired"];
    }

    /// <summary>
    /// Error message indicating that permission description is required.
    /// </summary>
    public string PermissionDescriptionRequired()
    {
        return localizer["PermissionDescriptionRequired"];
    }

    /// <summary>
    /// Error message indicating that role name is required.
    /// </summary>
    public string RoleNameRequired()
    {
        return localizer["RoleNameRequired"];
    }

    /// <summary>
    /// Error message indicating that role description is required.
    /// </summary>
    public string RoleDescriptionRequired()
    {
        return localizer["RoleDescriptionRequired"];
    }

    /// <summary>
    /// Error message indicating that the user account is already verified.
    /// </summary>
    public string AccountAlreadyVerified()
    {
        return localizer["AccountAlreadyVerified"];
    }

    /// <summary>
    /// Error message indicating that no valid OTP was found for verification.
    /// </summary>
    public string NoValidOtpFound()
    {
        return localizer["NoValidOtpFound"];
    }

    /// <summary>
    /// Error message indicating that the provided OTP code is invalid.
    /// </summary>
    public string InvalidOtpCode()
    {
        return localizer["InvalidOtpCode"];
    }

    /// <summary>
    /// Error message indicating that the OTP has expired.
    /// </summary>
    public string OtpExpired()
    {
        return localizer["OtpExpired"];
    }

    /// <summary>
    /// Error message indicating that maximum OTP verification attempts have been reached.
    /// </summary>
    public string MaxOtpAttemptsReached()
    {
        return localizer["MaxOtpAttemptsReached"];
    }

    /// <summary>
    /// Error message indicating that the OTP has not been verified yet.
    /// </summary>
    public string OtpNotYetVerified()
    {
        return localizer["OtpNotYetVerified"];
    }

    /// <summary>
    /// Error message indicating that the new password cannot be the same as the old password.
    /// </summary>
    public string NewPasswordSameAsOld()
    {
        return localizer["NewPasswordSameAsOld"];
    }

    /// <summary>
    /// Error message indicating that a password has not been configured for the account.
    /// </summary>
    /// <param name="provider">The OAuth provider name (e.g., Google, Facebook).</param>
    /// <returns>
    /// A formatted error message indicating that the user must set a password first.
    /// </returns>
    public string PasswordNotConfigured(EnumAuthProvider provider)
    {
        return string.Format(localizer["PasswordNotConfigured"], provider);
    }

    /// <summary>
    /// Error message indicating that the current password is incorrect.
    /// </summary>
    /// <returns>
    /// An error message indicating that the provided current password does not match.
    /// </returns>
    public string IncorrectCurrentPassword()
    {
        return localizer["IncorrectCurrentPassword"];
    }

    /// <summary>
    /// Error message indicating that an email address is required to set a password.
    /// </summary>
    /// <returns>
    /// An error message indicating that the user must have an email address before setting a password.
    /// </returns>
    public string EmailRequiredToSetPassword()
    {
        return localizer["EmailRequiredToSetPassword"];
    }

    /// <summary>
    /// Error message indicating that setting password is only allowed for external auth users.
    /// </summary>
    /// <returns>
    /// An error message indicating that only Google or Facebook users can set a password.
    /// </returns>
    public string PasswordOnlyForExternalAuth()
    {
        return localizer["PasswordOnlyForExternalAuth"];
    }

    /// <summary>
    /// Gets an error message for when a core role cannot be deleted.
    /// </summary>
    /// <param name="roleName">The name of the core role.</param>
    /// <returns>
    /// A formatted error message indicating that the core role cannot be deleted.
    /// </returns>
    public string CoreRoleCannotBeDeleted(string roleName)
    {
        return string.Format(localizer["CoreRoleCannotBeDeleted"], roleName);
    }

    /// <summary>
    /// Error message indicating that a role is inactive and cannot be assigned.
    /// </summary>
    public string RoleIsInactive()
    {
        return localizer["RoleIsInactive"];
    }

    /// <summary>
    /// Error message indicating that a role is deleted and cannot be used.
    /// </summary>
    public string RoleIsDeleted()
    {
        return localizer["RoleIsDeleted"];
    }

    /// <summary>
    /// Error message indicating that a permission is inactive.
    /// </summary>
    public string PermissionIsInactive()
    {
        return localizer["PermissionIsInactive"];
    }

    /// <summary>
    /// Error message indicating that a permission is deleted.
    /// </summary>
    public string PermissionIsDeleted()
    {
        return localizer["PermissionIsDeleted"];
    }

    /// <summary>
    /// Error message indicating that a permission is not assigned to the role.
    /// </summary>
    public string PermissionNotAssignedToRole()
    {
        return localizer["PermissionNotAssignedToRole"];
    }

    /// <summary>
    /// Error message indicating that a role is not assigned to the user.
    /// </summary>
    public string RoleNotAssignedToUser()
    {
        return localizer["RoleNotAssignedToUser"];
    }

    /// <summary>
    /// Error message indicating that one or more export column names are invalid.
    /// </summary>
    /// <param name="validColumns">Comma-separated list of valid column names.</param>
    public string ExportColumnsInvalid(string validColumns) =>
        string.Format(localizer["ExportColumnsInvalid"], validColumns);

    /// <summary>
    /// Error message indicating that the export format is invalid.
    /// </summary>
    public string ExportFormatInvalid() => localizer["ExportFormatInvalid"];

    /// <summary>
    /// Error message indicating that the export session status filter is invalid.
    /// </summary>
    public string ExportStatusInvalid() => localizer["ExportStatusInvalid"];

    /// <summary>
    /// Error message indicating that ToDate must be greater than or equal to FromDate.
    /// </summary>
    public string ExportDateRangeInvalid() => localizer["ExportDateRangeInvalid"];
}
