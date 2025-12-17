namespace _116.BuildingBlocks.Constants;

/// <summary>
/// Constants for session management and refresh token handling.
/// </summary>
public static class SessionConstants
{
    /// <summary>
    /// How long a refresh token is valid (in days).
    /// After this time, the user needs to log in again.
    /// </summary>
    public const int RefreshTokenExpirationDays = 30;

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
    /// Maximum length for parsed device name.
    /// </summary>
    public const int MaxDeviceNameLength = 100;
}
