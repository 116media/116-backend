using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;

/// <summary>
/// Pages an artist's published lyrics and videos for the public artist-by-slug query.
/// </summary>
/// <param name="lyricsRepository">Repository paging the published lyrics.</param>
/// <param name="videoRepository">Repository paging the published videos.</param>
/// <param name="lyricsDtoService">Service assembling the public lyrics summaries.</param>
/// <param name="videoDtoService">Service assembling the public video summaries.</param>
public class PublicArtistPageService(
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    ILyricsDtoService lyricsDtoService,
    IVideoDtoService videoDtoService
) : IPublicArtistPageService
{
    /// <inheritdoc />
    public async Task<PaginatedResult<PublicLyricsSummaryDto>> GetLyricsPageAsync(
        Guid artistId,
        PaginatedRequest page,
        CancellationToken cancellationToken
    )
    {
        (List<LyricsEntity> lyrics, int totalCount) = await lyricsRepository.GetPublishedByArtistAsync(
            artistId: artistId,
            page: page.PageIndex + 1,
            pageSize: page.PageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicLyricsSummaryDto> items = await lyricsDtoService.CreatePublicManyAsync(
            lyrics,
            cancellationToken
        );

        return new PaginatedResult<PublicLyricsSummaryDto>(
            pageIndex: page.PageIndex,
            pageSize: page.PageSize,
            count: totalCount,
            items: items
        );
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<PublicVideoSummaryDto>> GetVideosPageAsync(
        Guid artistId,
        PaginatedRequest page,
        CancellationToken cancellationToken
    )
    {
        (List<VideoEntity> videos, int totalCount) = await videoRepository.GetPublishedByArtistAsync(
            artistId: artistId,
            page: page.PageIndex + 1,
            pageSize: page.PageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> items = await videoDtoService.CreatePublicManyAsync(
            videos,
            cancellationToken
        );

        return new PaginatedResult<PublicVideoSummaryDto>(
            pageIndex: page.PageIndex,
            pageSize: page.PageSize,
            count: totalCount,
            items: items
        );
    }
}
