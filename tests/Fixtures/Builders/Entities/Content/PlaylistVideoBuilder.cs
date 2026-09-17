using System.Reflection;
using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PlaylistVideoEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; no factory wraps it yet.
/// </summary>
public class PlaylistVideoBuilder
{
    private readonly PlaylistEntity _playlist;
    private Guid _videoId = Guid.NewGuid();
    private int _sortOrder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistVideoBuilder"/> class for a playlist.
    /// </summary>
    public PlaylistVideoBuilder(PlaylistEntity playlist)
    {
        _playlist = playlist;
    }

    /// <summary>
    /// Sets the position of the video within the playlist.
    /// </summary>
    public PlaylistVideoBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    /// <summary>
    /// Attaches the Video navigation EF Core populates through <c>.Include(l =&gt; l.Video)</c>,
    /// and points the foreign key at the same video.
    /// </summary>
    public PlaylistVideoBuilder WithVideo(VideoEntity video)
    {
        _videoId = video.Id;
        _videoId = video.Id;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PlaylistVideoEntity" /> instance.
    /// </summary>
    public PlaylistVideoEntity Build()
    {
        _playlist.AddVideo(videoId: _videoId, sortOrder: _sortOrder);
        PlaylistVideoEntity link = _playlist.Videos.First(entry => entry.VideoId == _videoId);

        return link;
    }
}
