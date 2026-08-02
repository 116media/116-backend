using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="StreamingLinkEntity"/> instances in tests.
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

    /// <summary>
    /// Creates a curated streaming link with a specific ID for the given album and platform.
    /// </summary>
    public static StreamingLinkEntity CreateForAlbumWithId(
        Guid id,
        Guid albumId,
        EnumStreamingPlatform platform = EnumStreamingPlatform.Spotify
    ) => new StreamingLinkBuilder().WithId(id).ForAlbum(albumId).WithPlatform(platform).Build();

    /// <summary>
    /// Creates a curated streaming link with a specific ID for the given lyrics page and platform.
    /// </summary>
    public static StreamingLinkEntity CreateForLyricsWithId(
        Guid id,
        Guid lyricsId,
        EnumStreamingPlatform platform = EnumStreamingPlatform.Spotify
    ) => new StreamingLinkBuilder().WithId(id).ForLyrics(lyricsId).WithPlatform(platform).Build();
}
