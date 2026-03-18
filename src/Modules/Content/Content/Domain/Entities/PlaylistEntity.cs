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
}
