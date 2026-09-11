using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Builds <see cref="ArtistDto" /> projections, resolving each artist's avatar in a single batch
/// so callers never issue one file query per artist.
/// </summary>
public interface IArtistDtoFactory
{
    /// <summary>
    /// Builds the projection for one artist.
    /// </summary>
    /// <param name="artist">The artist to project.</param>
    /// <param name="socialLinks">The artist's social links, when the caller loaded them.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<ArtistDto> CreateAsync(
        ArtistEntity artist,
        IReadOnlyList<ArtistSocialLinkEntity>? socialLinks = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the projections for a list of artists, resolving every avatar in one query.
    /// </summary>
    /// <param name="artists">The artists to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<ArtistDto>> CreateManyAsync(IReadOnlyList<ArtistEntity> artists, CancellationToken ct = default);
}
