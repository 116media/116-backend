using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="StreamingLinkBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class StreamingLinkFactory
{
    /// <summary>
    /// Creates a curated streaming link for the given album and platform.
    /// </summary>
    public static StreamingLinkEntity CreateForAlbum(
        Guid albumId,
        EnumStreamingPlatform platform = EnumStreamingPlatform.Spotify,
        string? url = null
    ) =>
        new StreamingLinkBuilder()
            .ForAlbum(albumId)
            .WithPlatform(platform)
            .WithUrl(url ?? "https://open.spotify.com/album/curated-abc123")
            .Build();

    /// <summary>
    /// Creates a curated streaming link for the given standalone single (lyrics page) and platform.
    /// </summary>
    public static StreamingLinkEntity CreateForLyrics(
        Guid lyricsId,
        EnumStreamingPlatform platform = EnumStreamingPlatform.Spotify,
        string? url = null
    ) =>
        new StreamingLinkBuilder()
            .ForLyrics(lyricsId)
            .WithPlatform(platform)
            .WithUrl(url ?? "https://open.spotify.com/track/curated-xyz789")
            .Build();
}
