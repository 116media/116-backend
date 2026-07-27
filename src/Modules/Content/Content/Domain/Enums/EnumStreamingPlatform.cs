namespace _116.Content.Domain.Enums;

/// <summary>
/// Defines the streaming platforms a <see cref="Entities.StreamingLinkEntity" /> can target.
/// Every album or standalone single resolves exactly one link per platform — either a
/// curated deep link or a generated search-query fallback.
/// </summary>
public enum EnumStreamingPlatform
{
    /// <summary>
    /// Spotify — track or album page, or a search-query fallback URL.
    /// </summary>
    Spotify,

    /// <summary>
    /// Apple Music — track or album page, or a search-query fallback URL.
    /// </summary>
    AppleMusic,

    /// <summary>
    /// YouTube Music — track or album page, or a search-query fallback URL.
    /// </summary>
    YoutubeMusic,

    /// <summary>
    /// Tidal — track or album page, or a search-query fallback URL.
    /// </summary>
    Tidal,

    /// <summary>
    /// Deezer — track or album page, or a search-query fallback URL. Appended last because
    /// the stored value is the member's integer; members are never reordered.
    /// </summary>
    Deezer,
}
