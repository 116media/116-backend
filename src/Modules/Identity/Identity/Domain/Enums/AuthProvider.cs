namespace _116.Identity.Domain.Enums;

/// <summary>
/// Defines the supported authentication providers for a user.
/// </summary>
/// <remarks>
/// This enumeration is typically used to determine how a user was authenticated,
/// whether by the application's local identity system or by an external identity provider.
/// </remarks>
public enum EnumAuthProvider
{
    /// <summary>
    /// Authentication via local credentials (username/password).
    /// </summary>
    Local,
    /// <summary>
    /// Authentication via Google.
    /// </summary>
    Google,
    /// <summary>
    /// Authentication via Facebook.
    /// </summary>
    Facebook,
}
