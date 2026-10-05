using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;

/// <summary>
/// Handles the <see cref="PublicGetArtistBySlugQuery" /> to serve an artist profile with its
/// totals and a page each of its published lyrics and videos.
/// </summary>
/// <param name="artistRepository">Repository resolving the artist and its totals.</param>
/// <param name="artistDtoService">Service assembling the artist DTO.</param>
/// <param name="pageService">Service paging the artist's lyrics and videos.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetArtistBySlugHandler(
    IArtistRepository artistRepository,
    IArtistDtoService artistDtoService,
    IPublicArtistPageService pageService,
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

        // An empty profile is not served, so unclaimed stubs never become crawlable dead pages.
        if (totals.Songs + totals.Videos + totals.Albums + totals.Mixtapes + totals.News == 0)
        {
            throw i18n.Artist.NotFound(id: artist.Id);
        }

        IReadOnlyList<ArtistSocialLinkEntity> socialLinks = artist.SocialLinks.OrderBy(link => link.Platform).ToList();
        ArtistDto artistDto = await artistDtoService.CreateAsync(artist, socialLinks, cancellationToken);

        PaginatedResult<PublicLyricsSummaryDto> lyrics = await pageService.GetLyricsPageAsync(
            artistId: artist.Id,
            page: query.LyricsPage,
            cancellationToken: cancellationToken
        );
        PaginatedResult<PublicVideoSummaryDto> videos = await pageService.GetVideosPageAsync(
            artistId: artist.Id,
            page: query.VideosPage,
            cancellationToken: cancellationToken
        );

        var totalsDto = new ArtistTotalsDto(
            Songs: totals.Songs,
            Videos: totals.Videos,
            Albums: totals.Albums,
            Mixtapes: totals.Mixtapes,
            News: totals.News
        );

        return new PublicGetArtistBySlugResult(Artist: artistDto, Totals: totalsDto, Lyrics: lyrics, Videos: videos);
    }
}
