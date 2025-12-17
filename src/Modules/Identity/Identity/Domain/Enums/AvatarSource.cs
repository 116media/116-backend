namespace _116.Auth.Domain.Enums;

/// <summary>
/// Defines the source of a user's avatar.
/// </summary>
public enum EnumAvatarSource
{
    /// <summary>
    /// Avatar manually uploaded by user or admin.
    /// </summary>
    Manual,

    /// <summary>
    /// Avatar from authentication provider (Google, Facebook, etc.).
    /// </summary>
    Provider,

    /// <summary>
    /// No avatar is set.
    /// This is the default when the user has not uploaded an avatar
    /// and no provider image is available.
    /// </summary>
    None,
}
