using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a user-created playlist of videos.
/// Users can create, rename, and delete their own playlists, and add/remove videos.
/// </summary>
public class PlaylistEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID who owns this playlist. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The display name of the playlist.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxPlaylistNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The videos contained in this playlist, ordered by SortOrder.
    /// </summary>
    public ICollection<PlaylistVideoEntity> Videos { get; } = new List<PlaylistVideoEntity>();

    private PlaylistEntity() { }

    /// <summary>
    /// Creates a new playlist for a user.
    /// </summary>
    /// <param name="id">The unique identifier for the playlist.</param>
    /// <param name="userId">The identity user UUID who owns this playlist.</param>
    /// <param name="name">The display name of the playlist.</param>
    /// <returns>A new <see cref="PlaylistEntity" />.</returns>
    public static PlaylistEntity Create(Guid id, Guid userId, string name)
    {
        return new PlaylistEntity
        {
            Id = id,
            UserId = userId,
            Name = name,
        };
    }

    /// <summary>
    /// Renames the playlist.
    /// </summary>
    /// <param name="name">The new display name.</param>
    public void Rename(string name) => Name = name;

    /// <summary>
    /// Adds a video to this playlist, reporting false when it is already there.
    /// </summary>
    /// <param name="videoId">The video to add.</param>
    /// <param name="sortOrder">The position of the video within the playlist.</param>
    /// <returns><c>true</c> if the video was added; otherwise <c>false</c>.</returns>
    public bool AddVideo(Guid videoId, int sortOrder)
    {
        if (ContainsVideo(videoId: videoId))
        {
            return false;
        }

        Videos.Add(
            PlaylistVideoEntity.Create(id: Guid.NewGuid(), playlistId: Id, videoId: videoId, sortOrder: sortOrder)
        );

        return true;
    }

    /// <summary>
    /// Removes a video from this playlist, reporting whether it was there.
    /// </summary>
    /// <param name="videoId">The video to remove.</param>
    /// <returns><c>true</c> if the video was removed; otherwise <c>false</c>.</returns>
    public bool RemoveVideo(Guid videoId)
    {
        PlaylistVideoEntity? entry = Videos.FirstOrDefault(video => video.VideoId == videoId);

        if (entry is null)
        {
            return false;
        }

        Videos.Remove(entry);

        return true;
    }

    /// <summary>
    /// Reports whether this playlist already contains a video.
    /// </summary>
    /// <param name="videoId">The video to look for.</param>
    /// <returns><c>true</c> if the video is in the playlist; otherwise <c>false</c>.</returns>
    public bool ContainsVideo(Guid videoId)
    {
        return Videos.Any(video => video.VideoId == videoId);
    }
}
