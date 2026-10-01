namespace _116.Content.Domain.Entities;

/// <summary>
/// Membership behaviour of <see cref="PlaylistEntity" />. Its state lives in <c>Entities/PlaylistEntity.cs</c>.
/// </summary>
public partial class PlaylistEntity
{
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
