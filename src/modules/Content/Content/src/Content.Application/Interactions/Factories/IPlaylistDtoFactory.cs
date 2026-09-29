using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Interactions.Factories;

/// <summary>
/// Builds playlist projections, resolving every video thumbnail in a single batch so callers never
/// issue one file query per video.
/// </summary>
public interface IPlaylistDtoFactory
{
    /// <summary>
    /// Builds the detail projection for one playlist, with every video's thumbnail resolved.
    /// </summary>
    /// <param name="playlist">The playlist to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The detail projection.</returns>
    Task<PlaylistDetailDto> CreateDetailAsync(PlaylistEntity playlist, CancellationToken ct = default);

    /// <summary>
    /// Builds the summary projections for a list of playlists, resolving every cover thumbnail in
    /// one query.
    /// </summary>
    /// <param name="playlists">The playlists to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<PlaylistDto>> CreateManyAsync(
        IReadOnlyList<PlaylistEntity> playlists,
        CancellationToken ct = default
    );
}
