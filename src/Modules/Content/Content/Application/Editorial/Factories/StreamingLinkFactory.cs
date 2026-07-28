using System.Net;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Builds the streaming platform deep links shown on an album or standalone single's public
/// detail page. Every platform always yields a link — a curated URL when one has been set
/// by an admin, otherwise a generated search-query fallback.
/// </summary>
public static class StreamingLinkFactory
{
    /// <summary>
    /// Creates the streaming platform links for a release. Always returns exactly one entry
    /// per <see cref="EnumStreamingPlatform" /> value — a curated link takes precedence over
    /// the generated fallback for that platform.
    /// </summary>
    /// <param name="artistName">The performing artist's display name.</param>
    /// <param name="releaseName">The release's display name (album name, or song title for a single).</param>
    /// <param name="curated">
    /// The curated streaming links already stored for this release, keyed by platform. Platforms
    /// absent from this dictionary fall back to a generated search URL.
    /// </param>
    /// <returns>
    /// A read-only list with exactly one <c>(Platform, Url)</c> tuple per streaming platform.
    /// </returns>
    public static IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> CreateStreamingLinks(
        string artistName,
        string releaseName,
        IReadOnlyDictionary<EnumStreamingPlatform, string> curated
    )
    {
        return Enum.GetValues<EnumStreamingPlatform>()
            .Select(platform =>
                (
                    Platform: platform,
                    Url: curated.TryGetValue(platform, out string? curatedUrl)
                        ? curatedUrl
                        : GenerateSearchUrl(platform: platform, artistName: artistName, releaseName: releaseName)
                )
            )
            .ToList();
    }

    /// <summary>
    /// Builds a generated search-query URL for a platform when no curated link exists.
    /// Internal rather than private so the unsupported-platform guard below stays directly
    /// testable — <see cref="CreateStreamingLinks" /> only ever feeds it declared enum members.
    /// </summary>
    internal static string GenerateSearchUrl(EnumStreamingPlatform platform, string artistName, string releaseName)
    {
        string query = WebUtility.UrlEncode($"{artistName} {releaseName}");

        // Streaming-affiliate revenue params (spec 12 §2) would be appended here per platform,
        // once each platform's affiliate program is confirmed active — not implemented yet.
        return platform switch
        {
            EnumStreamingPlatform.Spotify => $"https://open.spotify.com/search/{query}",
            EnumStreamingPlatform.AppleMusic => $"https://music.apple.com/search?term={query}",
            EnumStreamingPlatform.YoutubeMusic => $"https://music.youtube.com/search?q={query}",
            EnumStreamingPlatform.Tidal => $"https://listen.tidal.com/search?q={query}",
            EnumStreamingPlatform.Deezer => $"https://www.deezer.com/search/{query}",
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, "Unsupported streaming platform."),
        };
    }
}
