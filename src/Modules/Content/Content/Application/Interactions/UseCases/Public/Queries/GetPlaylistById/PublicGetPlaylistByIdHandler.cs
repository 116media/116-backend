using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetPlaylistById;

/// <summary>
/// Handles the <see cref="PublicGetPlaylistByIdQuery" /> to retrieve a playlist with its videos.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetPlaylistByIdHandler(IPlaylistRepository playlistRepository, IMapper mapper, ContentI18n i18n)
    : IQueryHandler<PublicGetPlaylistByIdQuery, PublicGetPlaylistByIdResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPlaylistByIdResult> Handle(
        PublicGetPlaylistByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        PlaylistEntity? playlist = await playlistRepository.GetByIdWithVideosAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        if (playlist is not null)
        {
            if (playlist.UserId != query.UserId)
            {
                throw i18n.Playlist.NotOwner();
            }

            var dto = playlist.ToPlaylistDetailDto(mapper);
            return new PublicGetPlaylistByIdResult(Playlist: dto);
        }

        throw i18n.Playlist.NotFound(id: query.Id);
    }
}
