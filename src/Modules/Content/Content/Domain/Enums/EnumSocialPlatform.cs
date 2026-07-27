namespace _116.Content.Domain.Enums;

/// <summary>
/// Defines the social platforms an <see cref="Entities.ArtistSocialLinkEntity" /> can target.
/// The stored value is the member's integer, so members are only ever appended — reordering
/// renumbers live rows. Clients skip values they do not recognise, which makes adding a
/// platform here before the client ships an icon safe.
/// </summary>
public enum EnumSocialPlatform
{
    /// <summary>
    /// Instagram profile.
    /// </summary>
    Instagram,

    /// <summary>
    /// X (formerly Twitter) profile.
    /// </summary>
    X,

    /// <summary>
    /// Facebook page or profile.
    /// </summary>
    Facebook,

    /// <summary>
    /// YouTube channel.
    /// </summary>
    YouTube,

    /// <summary>
    /// TikTok profile.
    /// </summary>
    TikTok,

    /// <summary>
    /// The artist's official website. One more outbound destination with an icon and a
    /// label, not a separate column on the artist row.
    /// </summary>
    Website,
}
