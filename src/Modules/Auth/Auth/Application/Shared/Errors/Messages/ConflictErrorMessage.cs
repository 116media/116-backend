namespace _116.Auth.Application.Shared.Errors.Messages;

/// <summary>
/// Provides conflict-related error messages for the <c>User</c> domain.
/// These messages describe situations where an entity already exists
/// and a create operation cannot proceed.
/// </summary>
public static class ConflictErrorMessage
{
    /// <summary>
    /// Gets an error message for when a user with the given email
    /// already exists.
    /// </summary>
    /// <param name="email">The email address that caused the conflict.</param>
    /// <returns>
    /// A formatted error message indicating that a user with the
    /// specified email already exists.
    /// </returns>
    public static string EmailAlreadyExists(string email)
    {
        return $"User with email '{email}' already exists";
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
    public static string UsernameAlreadyExists(string username)
    {
        return $"Username '{username}' is already taken";
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
    public static string PhoneNumberAlreadyExists(string phoneNumber)
    {
        return $"Phone number '{phoneNumber}' is already taken";
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
    public static string RoleAlreadyExists(string name)
    {
        return $"Role '{name}' already exists";
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
    public static string PermissionAlreadyExists(string resource, string action)
    {
        return $"Permission '{resource}.{action}' already exists";
    }

    /// <summary>
    /// Gets an error message for when a role is already assigned to a user.
    /// </summary>
    /// <returns>
    /// A formatted error message indicating that the role is already assigned to the user.
    /// </returns>
    public static string RoleAlreadyAssignedToUser()
    {
        return "Role is already assigned to this user";
    }
}
