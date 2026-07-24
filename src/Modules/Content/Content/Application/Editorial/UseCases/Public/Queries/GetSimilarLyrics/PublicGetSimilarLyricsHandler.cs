using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics;

/// <summary>
/// Handles the <see cref="PublicGetSimilarLyricsQuery" /> to retrieve lyrics pages similar to
/// a given lyrics page, resolved via <see cref="ILyricsRepository.GetSimilarAsync" />'s
/// three-way waterfall (spec 06).
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
public class PublicGetSimilarLyricsHandler(ILyricsRepository lyricsRepository, IFileRepository fileRepository)
    : IQueryHandler<PublicGetSimilarLyricsQuery, PublicGetSimilarLyricsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetSimilarLyricsResult> Handle(
        PublicGetSimilarLyricsQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<LyricsEntity> similar = await lyricsRepository.GetSimilarAsync(
            lyricsId: query.LyricsId,
            cancellationToken: cancellationToken
        );

        List<Guid> lyricsIds = similar.Select(lyrics => lyrics.Id).ToList();
        IReadOnlySet<Guid> likedLyricsIds = await lyricsRepository.GetLikedIdsAsync(
            currentUserId: query.CurrentUserId,
            lyricsIds: lyricsIds,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<LyricsSummaryDto> dtoList = await similar.ToLyricsSummaryDtosAsync(
            fileRepository,
            likedLyricsIds,
            cancellationToken
        );

        return new PublicGetSimilarLyricsResult(Lyrics: dtoList);
    }
}
