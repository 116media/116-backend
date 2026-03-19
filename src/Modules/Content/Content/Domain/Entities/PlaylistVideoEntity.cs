using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking a playlist to a video, with display sort order.
/// </summary>
public class PlaylistVideoEntity : Aggregate<Guid>
{
    /// <summary>
    /// The playlist this entry belongs to.
    /// </summary>
    public Guid PlaylistId { get; private set; }

    /// <summary>
    /// The video included in the playlist.
    /// </summary>
    public Guid VideoId { get; private set; }

    /// <summary>
    /// The display order of this video within the playlist. Lower values come first.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Navigation property to the playlist.
    /// </summary>
    public PlaylistEntity Playlist { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the video.
    /// </summary>
    public VideoEntity Video { get; private set; } = null!;

    private PlaylistVideoEntity() { }

    /// <summary>
    /// Creates a new playlist-video entry.
    /// </summary>
    /// <param name="id">The unique identifier for this entry.</param>
    /// <param name="playlistId">The playlist the video is being added to.</param>
    /// <param name="videoId">The video being added.</param>
    /// <param name="sortOrder">The display order within the playlist.</param>
    /// <returns>A new <see cref="PlaylistVideoEntity" />.</returns>
    public static PlaylistVideoEntity Create(Guid id, Guid playlistId, Guid videoId, int sortOrder)
    {
        return new PlaylistVideoEntity
        {
            Id = id,
            PlaylistId = playlistId,
            VideoId = videoId,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Updates the display sort order of this video within the playlist.
    /// </summary>
    /// <param name="sortOrder">The new sort order value.</param>
    public void UpdateSortOrder(int sortOrder) => SortOrder = sortOrder;
}
