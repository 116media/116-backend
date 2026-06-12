using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Handles the <see cref="PublicGetLyricsByVideoIdQuery" /> to retrieve
/// the lyrics page linked to a given video.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetLyricsByVideoIdHandler(ILyricsRepository lyricsRepository, IMapper mapper, ContentI18n i18n)
    : IQueryHandler<PublicGetLyricsByVideoIdQuery, PublicGetLyricsByVideoIdResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsByVideoIdResult> Handle(
        PublicGetLyricsByVideoIdQuery query,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(query.VideoId);

        LyricsEntity? lyrics = await lyricsRepository.GetByVideoIdAsync(
            videoId: videoId,
            cancellationToken: cancellationToken
        );

        if (lyrics is not null)
        {
            var dto = lyrics.ToLyricsDto(mapper);
            return new PublicGetLyricsByVideoIdResult(Lyrics: dto);
        }

        throw i18n.Lyrics.NotFound(id: videoId);
    }
}
