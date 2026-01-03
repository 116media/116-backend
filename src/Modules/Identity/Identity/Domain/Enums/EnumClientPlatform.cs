namespace _116.Identity.Domain.Enums;

/// <summary>
/// Defines the client platform identifiers sent via Client-Platform header.
/// </summary>
public enum EnumClientPlatform
{
    /// <summary>
    /// iOS mobile application.
    /// </summary>
    IosMobile,

    /// <summary>
    /// Android mobile application.
    /// </summary>
    AndroidMobile,

    /// <summary>
    /// Web browser application.
    /// </summary>
    BrowserWeb,

    /// <summary>
    /// Progressive Web App (PWA).
    /// </summary>
    PwaBrowser,

    /// <summary>
    /// Unknown or missing client platform.
    /// </summary>
    Unknown
}
