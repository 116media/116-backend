using Microsoft.Extensions.Localization;

namespace _116.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Provides authorization-related error messages for the <c>User</c> domain.
/// These messages describe failures related to account status or permission
/// restrictions.
/// </summary>
public class AuthorizationErrorMessage(IStringLocalizer<AuthorizationErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when an account is inactive.
    /// </summary>
    /// <param name="email">The email of the inactive account.</param>
    /// <returns>
    /// A formatted error message indicating that the specified account is inactive.
    /// </returns>
    public string AccountInactive(string email)
    {
        return string.Format(localizer["AccountInactive"], email);
    }

    /// <summary>
    /// Gets an error message for when an account is not verified.
    /// </summary>
    /// <param name="email">The email of the unverified account.</param>
    /// <returns>
    /// A formatted error message indicating that the specified account is not verified.
    /// </returns>
    public string AccountNotVerified(string email)
    {
        return string.Format(localizer["AccountNotVerified"], email);
    }

    /// <summary>
    /// Error message indicating that the social provider reports the account email as not verified.
    /// </summary>
    public string ProviderEmailNotVerified()
    {
        return localizer["ProviderEmailNotVerified"];
    }

    /// <summary>
    /// Error message indicating that access is denied due to insufficient permissions.
    /// </summary>
    public string AccessDenied()
    {
        return localizer["AccessDenied"];
    }
}
