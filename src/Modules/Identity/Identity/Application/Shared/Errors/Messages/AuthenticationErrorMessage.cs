namespace _116.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Provides authentication-related error messages for the <c>User</c> domain.
/// These messages describe authentication failures, such as invalid credentials
/// or missing administrative privileges.
/// </summary>
public static class AuthenticationErrorMessage
{
    /// <summary>
    /// Generic error message indicating that the provided login credentials
    /// are invalid. This message avoids leaking sensitive information
    /// for security reasons.
    /// </summary>
    public static string InvalidCredentials()
    {
        return "Invalid email or password";
    }

    /// <summary>
    /// Error message indicating that the user is not authenticated or the user ID is invalid.
    /// </summary>
    public static string InvalidUserAuthentication()
    {
        return "User not authenticated or invalid user ID";
    }

    /// <summary>
    /// Error message indicating that the user does not have sufficient permissions for this operation.
    /// </summary>
    public static string InsufficientPermissions()
    {
        return "Access Denied. Insufficient permissions for this operation";
    }

    /// <summary>
    /// Error message indicating that JWT Bearer token authentication is required.
    /// </summary>
    public static string JwtTokenRequired()
    {
        return "Authentication required. Please provide a valid JWT Bearer token";
    }
}
