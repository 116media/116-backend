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
    private Guid _id = Guid.NewGuid();
    private Guid _playlistId = Guid.NewGuid();
    private Guid _videoId = Guid.NewGuid();
    private int _sortOrder;
    private VideoEntity? _video;

    /// <summary>
    /// Sets the playlist the video is linked into.
    /// </summary>
    public PlaylistVideoBuilder WithPlaylistId(Guid playlistId)
    {
        _playlistId = playlistId;
        return this;
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
        _video = video;
        _videoId = video.Id;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PlaylistVideoEntity" /> instance.
    /// </summary>
    public PlaylistVideoEntity Build()
    {
        PlaylistVideoEntity link = PlaylistVideoEntity.Create(_id, _playlistId, _videoId, _sortOrder);

        if (_video is not null)
        {
            typeof(PlaylistVideoEntity)
                .GetProperty(nameof(PlaylistVideoEntity.Video), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(link, _video);
        }

        return link;
    }
}
