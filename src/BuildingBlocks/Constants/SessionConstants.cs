namespace _116.BuildingBlocks.Constants;

/// <summary>
/// Constants for session management and refresh token handling.
/// </summary>
public static class SessionConstants
{
    /// <summary>
    /// Maximum length for the hashed refresh token.
    /// </summary>
    public const int MaxRefreshTokenHashLength = 500;

    /// <summary>
    /// Maximum length for IP address (supports both IPv4 and IPv6).
    /// </summary>
    public const int MaxIpAddressLength = 45;

    /// <summary>
    /// Maximum length for user agent string.
    /// </summary>
    public const int MaxUserAgentLength = 500;

    /// <summary>
    /// Maximum length for client platform identifier.
    /// Allowed values: "ios-mobile", "android-mobile", "browser-web", "pwa-browser"
    /// </summary>
    public const int MaxClientPlatformLength = 25;

    /// <summary>
    /// Maximum length for device identifier.
    /// Used to store client-generated unique device ID (GUID/UUID/NanoID).
    /// </summary>
    public const int MaxDeviceIdLength = 64;
}
