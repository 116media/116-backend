namespace _116.Identity.Domain.Enums;

/// <summary>
/// Defines the application client type that initiated the session.
/// </summary>
public enum EnumClient
{
    /// <summary>
    /// Mobile application (iOS or Android native app).
    /// </summary>
    MobileApp,

    /// <summary>
    /// Web application (frontend SPA).
    /// </summary>
    WebApp,

    /// <summary>
    /// Admin dashboard application.
    /// </summary>
    Dashboard,

    /// <summary>
    /// Unknown or unspecified client application.
    /// </summary>
    Unknown,
}
