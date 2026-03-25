using _116.Identity.Domain.Results;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Service for managing token delivery based on client type.
/// Web clients (Dashboard, WebApp) receive tokens via HttpOnly cookies,
/// while mobile clients receive tokens in the JSON response body.
/// </summary>
public interface ITokenDeliveryService
{
    /// <summary>
    /// Determines whether the current request originates from a web client (Dashboard or WebApp).
    /// </summary>
    /// <returns>
    /// <c>true</c> if the Client-App header indicates Dashboard or WebApp; otherwise <c>false</c>.
    /// </returns>
    bool IsWebClient();

    /// <summary>
    /// Sets access and refresh tokens as HttpOnly cookies on the current HTTP response.
    /// </summary>
    /// <param name="authResult">
    /// The authentication result containing tokens and expiration times.
    /// </param>
    void SetTokenCookies(AuthenticationResult authResult);

    /// <summary>
    /// Clears the access and refresh token cookies from the current HTTP response.
    /// Used during sign-out to invalidate browser-stored tokens.
    /// </summary>
    void ClearTokenCookies();

    /// <summary>
    /// Reads the refresh token from the appropriate source based on client type.
    /// Web clients provide the refresh token via HttpOnly cookie,
    /// while mobile clients provide it in the request body.
    /// </summary>
    /// <param name="bodyRefreshToken">
    /// The refresh token from the request body (used for mobile clients).
    /// </param>
    /// <returns>
    /// The refresh token string from the cookie (web) or body (mobile).
    /// </returns>
    string? ReadRefreshToken(string? bodyRefreshToken);
}
