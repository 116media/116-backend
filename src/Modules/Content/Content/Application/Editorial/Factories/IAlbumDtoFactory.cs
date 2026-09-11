using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Builds <see cref="AlbumDto" /> projections, resolving each album's cover in a single batch so
/// callers never issue one file query per album.
/// </summary>
public interface IAlbumDtoFactory
{
    /// <summary>
    /// Builds the projection for one album.
    /// </summary>
    /// <param name="album">The album to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<AlbumDto> CreateAsync(AlbumEntity album, CancellationToken ct = default);

    /// <summary>
    /// Builds the projections for a list of albums, resolving every cover in one query.
    /// </summary>
    /// <param name="albums">The albums to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<AlbumDto>> CreateManyAsync(IReadOnlyList<AlbumEntity> albums, CancellationToken ct = default);
}
