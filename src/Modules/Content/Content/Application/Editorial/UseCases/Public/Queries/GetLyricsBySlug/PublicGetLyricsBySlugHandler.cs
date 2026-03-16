using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Handles the <see cref="PublicGetLyricsBySlugQuery" /> to retrieve a lyrics page by song title and artist name.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetLyricsBySlugHandler(ILyricsRepository lyricsRepository, IMapper mapper)
    : IQueryHandler<PublicGetLyricsBySlugQuery, PublicGetLyricsBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsBySlugResult> Handle(
        PublicGetLyricsBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity? lyrics = await lyricsRepository.GetBySongTitleAndArtistAsync(
            songTitle: query.SongTitle,
            artistName: query.ArtistName,
            cancellationToken: cancellationToken
        );

        if (lyrics is null)
        {
            throw LyricsErrors.NotFound(id: Guid.Empty);
        }

        var dto = lyrics.ToLyricsDto(mapper);
        return new PublicGetLyricsBySlugResult(Lyrics: dto);
    }
}
