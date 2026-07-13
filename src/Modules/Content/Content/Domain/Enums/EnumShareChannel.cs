namespace _116.Content.Domain.Enums;

/// <summary>
/// The outbound channel a piece of content was shared to. Distinct from the Identity
/// module's <c>EnumPlatform</c> (operating system) and <c>EnumClient</c> (client app).
/// </summary>
public enum EnumShareChannel
{
    /// <summary>
    /// Shared to Facebook.
    /// </summary>
    Facebook,

    /// <summary>
    /// Shared to X (formerly Twitter).
    /// </summary>
    X,

    /// <summary>
    /// Shared to WhatsApp.
    /// </summary>
    WhatsApp,

    /// <summary>
    /// Copied to the clipboard.
    /// </summary>
    Clipboard,

    /// <summary>
    /// Shared through the native Web Share sheet.
    /// </summary>
    WebShare,
}
