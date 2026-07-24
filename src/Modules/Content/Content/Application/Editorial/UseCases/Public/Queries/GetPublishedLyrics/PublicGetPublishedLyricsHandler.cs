using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics;

/// <summary>
/// Handles the <see cref="PublicGetPublishedLyricsQuery" /> to retrieve a paginated list of published lyrics pages.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
public class PublicGetPublishedLyricsHandler(ILyricsRepository lyricsRepository, IFileRepository fileRepository)
    : IQueryHandler<PublicGetPublishedLyricsQuery, PublicGetPublishedLyricsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPublishedLyricsResult> Handle(
        PublicGetPublishedLyricsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<LyricsEntity> lyricsList, int totalCount) = await lyricsRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            status: EnumContentStatus.Published,
            categoryId: query.CategoryId,
            language: query.Language,
            sort: query.Sort,
            cancellationToken: cancellationToken
        );

        List<Guid> lyricsIds = lyricsList.Select(lyrics => lyrics.Id).ToList();
        IReadOnlySet<Guid> likedLyricsIds = await lyricsRepository.GetLikedIdsAsync(
            currentUserId: query.CurrentUserId,
            lyricsIds: lyricsIds,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<LyricsSummaryDto> dtoList = await lyricsList
            .AsReadOnly()
            .ToLyricsSummaryDtosAsync(fileRepository, likedLyricsIds, cancellationToken);

        var paginatedResult = new PaginatedResult<LyricsSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetPublishedLyricsResult(Lyrics: paginatedResult);
    }
}
