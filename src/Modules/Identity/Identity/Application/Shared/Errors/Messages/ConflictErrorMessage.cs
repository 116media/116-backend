using Microsoft.Extensions.Localization;

namespace _116.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Provides conflict-related error messages for the <c>User</c> domain.
/// These messages describe situations where an entity already exists
/// and a create operation cannot proceed.
/// </summary>
public class ConflictErrorMessage(IStringLocalizer<ConflictErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a user with the given email
    /// already exists.
    /// </summary>
    /// <param name="email">The email address that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a user with the
    /// specified email already exists.
    /// </returns>
    public string EmailAlreadyExists(string email)
    {
        return string.Format(localizer["EmailAlreadyExists"], email);
    }

    /// <summary>
    /// Gets an error message for when the given username
    /// is already taken.
    /// </summary>
    /// <param name="username">The username that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that the specified username
    /// is already taken.
    /// </returns>
    public string UsernameAlreadyExists(string username)
    {
        return string.Format(localizer["UsernameAlreadyExists"], username);
    }

    /// <summary>
    /// Gets an error message for when the given phone number
    /// is already taken.
    /// </summary>
    /// <param name="phoneNumber">The phone number that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that the specified phone number
    /// is already taken.
    /// </returns>
    public string PhoneNumberAlreadyExists(string phoneNumber)
    {
        return string.Format(localizer["PhoneNumberAlreadyExists"], phoneNumber);
    }

    /// <summary>
    /// Gets an error message for when a role with the given name
    /// already exists.
    /// </summary>
    /// <param name="name">The role name that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a role with the
    /// specified name already exists.
    /// </returns>
    public string RoleAlreadyExists(string name)
    {
        return string.Format(localizer["RoleAlreadyExists"], name);
    }

    /// <summary>
    /// Gets an error message for when a permission with the given
    /// resource and action already exists.
    /// </summary>
    /// <param name="resource">The resource associated with the permission.</param>
    /// <param name="action">The action associated with the permission.</param>
    /// <returns>
    /// A formatted error message indicating that the specified
    /// resource-action permission already exists.
    /// </returns>
    public string PermissionAlreadyExists(string resource, string action)
    {
        return string.Format(localizer["PermissionAlreadyExists"], resource, action);
    }

    /// <summary>
    /// Gets an error message for when a role is already assigned to a user.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the role is already assigned to the user.
    /// </returns>
    public string RoleAlreadyAssignedToUser()
    {
        return localizer["RoleAlreadyAssignedToUser"];
    }

    /// <summary>
    /// Error message indicating that the account is linked to a different social identity.
    /// </summary>
    public string ProviderMismatch()
    {
        return localizer["ProviderMismatch"];
    }

    /// <summary>
    /// Gets an error message for when a permission is already assigned to a role.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the permission is already assigned to the role.
    /// </returns>
    public string PermissionAlreadyAssignedToRole()
    {
        return localizer["PermissionAlreadyAssignedToRole"];
    }

    /// <summary>
    /// Gets an error message for when a role is already active.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the role is already active.
    /// </returns>
    public string RoleAlreadyActive()
    {
        return localizer["RoleAlreadyActive"];
    }

    /// <summary>
    /// Gets an error message for when a role is already inactive.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the role is already inactive.
    /// </returns>
    public string RoleAlreadyInactive()
    {
        return localizer["RoleAlreadyInactive"];
    }

    /// <summary>
    /// Gets an error message for when a role is already deleted.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the role is already deleted.
    /// </returns>
    public string RoleAlreadyDeleted()
    {
        return localizer["RoleAlreadyDeleted"];
    }

    /// <summary>
    /// Gets an error message for when a role is not deleted and cannot be restored.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the role is not deleted.
    /// </returns>
    public string RoleNotDeleted()
    {
        return localizer["RoleNotDeleted"];
    }

    /// <summary>
    /// Gets an error message for when a permission is already active.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the permission is already active.
    /// </returns>
    public string PermissionAlreadyActive()
    {
        return localizer["PermissionAlreadyActive"];
    }

    /// <summary>
    /// Gets an error message for when a permission is already inactive.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the permission is already inactive.
    /// </returns>
    public string PermissionAlreadyInactive()
    {
        return localizer["PermissionAlreadyInactive"];
    }

    /// <summary>
    /// Gets an error message for when a permission is already deleted.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the permission is already deleted.
    /// </returns>
    public string PermissionAlreadyDeleted()
    {
        return localizer["PermissionAlreadyDeleted"];
    }

    /// <summary>
    /// Gets an error message for when a permission is not deleted and cannot be restored.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the permission is not deleted.
    /// </returns>
    public string PermissionNotDeleted()
    {
        return localizer["PermissionNotDeleted"];
    }
}
