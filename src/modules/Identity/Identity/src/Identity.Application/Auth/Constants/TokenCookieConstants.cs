namespace _116.Identity.Application.Auth.Constants;

/// <summary>
/// Constants for HttpOnly token cookie names.
/// Shared between TokenDeliveryService and JWT OnMessageReceived event.
/// </summary>
public static class TokenCookieConstants
{
    /// <summary>
    /// Cookie name for the JWT access token.
    /// </summary>
    public const string AccessTokenCookie = "accessToken";

    /// <summary>
    /// Cookie name for the refresh token.
    /// </summary>
    public const string RefreshTokenCookie = "refreshToken";
}
