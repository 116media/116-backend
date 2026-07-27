using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;

/// <summary>
/// Handles the <see cref="PublicGetArtistBySlugQuery" /> to retrieve an artist's public
/// profile page and their published catalog.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">The Mapster mapper used for video tags.</param>
/// <param name="fileRepository">Repository for resolving avatar, cover, and thumbnail file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetArtistBySlugHandler(
    IArtistRepository artistRepository,
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    IMapper mapper,
    IFileRepository fileRepository,
    ContentI18n i18n
) : IQueryHandler<PublicGetArtistBySlugQuery, PublicGetArtistBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArtistBySlugResult> Handle(
        PublicGetArtistBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity? artist = await artistRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (artist is null)
        {
            throw i18n.Artist.NotFound(id: Guid.Empty);
        }

        ArtistTotals totals = await artistRepository.GetTotalsAsync(
            artistId: artist.Id,
            cancellationToken: cancellationToken
        );

        // A profile with zero items on every surface is not served: unclaimed staff-curated
        // stubs must not become crawlable dead pages carrying a real person's name. The check
        // runs before any mapping so nothing is resolved for a response about to be discarded.
        if (totals.Songs + totals.Videos + totals.Albums + totals.Mixtapes + totals.News == 0)
        {
            throw i18n.Artist.NotFound(id: artist.Id);
        }

        int lyricsPageSize = query.LyricsPage.PageSize;
        int lyricsPageIndex = query.LyricsPage.PageIndex;

        (List<LyricsEntity> lyricsList, int lyricsTotalCount) = await lyricsRepository.GetPublishedByArtistAsync(
            artistId: artist.Id,
            page: lyricsPageIndex + 1,
            pageSize: lyricsPageSize,
            cancellationToken: cancellationToken
        );

        int videosPageSize = query.VideosPage.PageSize;
        int videosPageIndex = query.VideosPage.PageIndex;

        (List<VideoEntity> videoList, int videosTotalCount) = await videoRepository.GetPublishedByArtistAsync(
            artistId: artist.Id,
            page: videosPageIndex + 1,
            pageSize: videosPageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArtistSocialLinkEntity> socialLinks = await artistRepository.GetSocialLinksAsync(
            artistId: artist.Id,
            cancellationToken: cancellationToken
        );

        ArtistDto artistDto = await artist.ToArtistDtoAsync(fileRepository, cancellationToken, socialLinks);

        IReadOnlyList<LyricsSummaryDto> lyricsDtos = await lyricsList
            .AsReadOnly()
            .ToLyricsSummaryDtosAsync(fileRepository, cancellationToken);

        IReadOnlyList<VideoSummaryDto> videoDtos = await videoList
            .AsReadOnly()
            .ToVideoSummaryDtosAsync(mapper, fileRepository, cancellationToken);

        var lyricsResult = new PaginatedResult<LyricsSummaryDto>(
            pageIndex: lyricsPageIndex,
            pageSize: lyricsPageSize,
            count: lyricsTotalCount,
            items: lyricsDtos
        );

        var videosResult = new PaginatedResult<VideoSummaryDto>(
            pageIndex: videosPageIndex,
            pageSize: videosPageSize,
            count: videosTotalCount,
            items: videoDtos
        );

        var totalsDto = new ArtistTotalsDto(
            Songs: totals.Songs,
            Videos: totals.Videos,
            Albums: totals.Albums,
            Mixtapes: totals.Mixtapes,
            News: totals.News
        );

        return new PublicGetArtistBySlugResult(
            Artist: artistDto,
            Totals: totalsDto,
            Lyrics: lyricsResult,
            Videos: videosResult
        );
    }
}
