using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnPlaylists;

/// <summary>
/// Handles the <see cref="PublicGetOwnPlaylistsQuery" /> to retrieve the user's playlists.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
public class PublicGetOwnPlaylistsHandler(
    IPlaylistRepository playlistRepository,
    IPlaylistDtoService playlistDtoService
) : IQueryHandler<PublicGetOwnPlaylistsQuery, PublicGetOwnPlaylistsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnPlaylistsResult> Handle(
        PublicGetOwnPlaylistsQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<PlaylistEntity> playlists = await playlistRepository.GetByUserIdAsync(
            userId: query.UserId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PlaylistDto> dtoList = await playlistDtoService.CreateManyAsync(playlists, cancellationToken);
        return new PublicGetOwnPlaylistsResult(Playlists: dtoList);
    }
}
