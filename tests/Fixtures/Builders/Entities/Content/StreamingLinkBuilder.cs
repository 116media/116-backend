using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="StreamingLinkEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; StreamingLinkFactory only names chains three or more tests share.
/// </summary>
public class StreamingLinkBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid? _albumId;
    private Guid? _lyricsId;
    private EnumStreamingPlatform _platform = EnumStreamingPlatform.Spotify;
    private string _url = "https://open.spotify.com/album/curated-abc123";

    /// <summary>
    /// Sets the streaming link ID.
    /// </summary>
    public StreamingLinkBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Targets the given album, clearing any lyrics target.
    /// </summary>
    public StreamingLinkBuilder ForAlbum(Guid albumId)
    {
        _albumId = albumId;
        _lyricsId = null;
        return this;
    }

    /// <summary>
    /// Targets the given standalone single (lyrics page), clearing any album target.
    /// </summary>
    public StreamingLinkBuilder ForLyrics(Guid lyricsId)
    {
        _lyricsId = lyricsId;
        _albumId = null;
        return this;
    }

    /// <summary>
    /// Sets the streaming platform.
    /// </summary>
    public StreamingLinkBuilder WithPlatform(EnumStreamingPlatform platform)
    {
        _platform = platform;
        return this;
    }

    /// <summary>
    /// Sets the curated deep link URL.
    /// </summary>
    public StreamingLinkBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="StreamingLinkEntity"/> instance.
    /// </summary>
    public StreamingLinkEntity Build()
    {
        StreamingLinkEntity entity = _albumId.HasValue
            ? StreamingLinkEntity.ForAlbum(_id, _albumId.Value, _platform, _url)
            : StreamingLinkEntity.ForSingle(_id, _lyricsId!.Value, _platform, _url);

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
