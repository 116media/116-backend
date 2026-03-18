using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists;

/// <summary>
/// Handles the <see cref="PublicGetMyPlaylistsQuery" /> to retrieve the user's playlists.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
public class PublicGetMyPlaylistsHandler(IPlaylistRepository playlistRepository, IMapper mapper)
    : IQueryHandler<PublicGetMyPlaylistsQuery, PublicGetMyPlaylistsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetMyPlaylistsResult> Handle(
        PublicGetMyPlaylistsQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<PlaylistEntity> playlists = await playlistRepository.GetByUserIdAsync(
            userId: query.UserId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PlaylistDto> dtoList = playlists.ToPlaylistDtos(mapper);
        return new PublicGetMyPlaylistsResult(Playlists: dtoList);
    }
}
