using _116.Content.Domain.Entities;

namespace _116.Content.Application.Interactions.Persistence;

/// <summary>
/// Repository interface for playlist data access operations.
/// </summary>
public interface IPlaylistRepository
{
    /// <summary>
    /// Adds a new playlist to the repository.
    /// </summary>
    Task AddAsync(PlaylistEntity playlist, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all playlists owned by the given user.
    /// </summary>
    Task<IReadOnlyList<PlaylistEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a playlist by its identifier, or null if not found.
    /// </summary>
    Task<PlaylistEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a playlist by its identifier with its videos eagerly loaded, or null if not found.
    /// </summary>
    Task<PlaylistEntity?> GetByIdWithVideosAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the given video is already part of the given playlist.
    /// </summary>
    Task<bool> VideoExistsInPlaylistAsync(Guid playlistId, Guid videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a playlist-video junction record to the repository.
    /// </summary>
    Task AddVideoAsync(PlaylistVideoEntity playlistVideo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a playlist-video junction record identified by the playlist and video IDs.
    /// </summary>
    Task RemoveVideoAsync(Guid playlistId, Guid videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing playlist as modified.
    /// </summary>
    void Update(PlaylistEntity playlist);

    /// <summary>
    /// Marks a playlist for deletion from the repository.
    /// </summary>
    void Delete(PlaylistEntity playlist);
}
