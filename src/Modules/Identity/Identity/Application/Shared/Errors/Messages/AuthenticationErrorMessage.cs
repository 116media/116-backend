using Microsoft.Extensions.Localization;

namespace _116.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Provides authentication-related error messages for the <c>User</c> domain.
/// These messages describe authentication failures, such as invalid credentials
/// or missing administrative privileges.
/// </summary>
public class AuthenticationErrorMessage(IStringLocalizer<AuthenticationErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Generic error message indicating that the provided login credentials
    /// are invalid. This message avoids leaking sensitive information
    /// for security reasons.
    /// </summary>
    public string InvalidCredentials()
    {
        return localizer["InvalidCredentials"];
    }

    /// <summary>
    /// Error message indicating that a social sign-in token could not be verified.
    /// </summary>
    public string InvalidProviderToken()
    {
        return localizer["InvalidProviderToken"];
    }

    /// <summary>
    /// Error message indicating that the user is not authenticated or the user ID is invalid.
    /// </summary>
    public string InvalidUserAuthentication()
    {
        return localizer["InvalidUserAuthentication"];
    }

    /// <summary>
    /// Error message indicating that the user does not have sufficient permissions for this operation.
    /// </summary>
    public string InsufficientPermissions()
    {
        return localizer["InsufficientPermissions"];
    }

    /// <summary>
    /// Error message indicating that JWT Bearer token authentication is required.
    /// </summary>
    public string JwtTokenRequired()
    {
        return localizer["JwtTokenRequired"];
    }

    /// <summary>
    /// Error message indicating that the provided refresh token is invalid or has expired.
    /// </summary>
    public string InvalidRefreshToken()
    {
        return localizer["InvalidRefreshToken"];
    }

    /// <summary>
    /// Error message indicating that the device identifier is missing from the request.
    /// </summary>
    public string DeviceIdRequired()
    {
        return localizer["DeviceIdRequired"];
    }
}
