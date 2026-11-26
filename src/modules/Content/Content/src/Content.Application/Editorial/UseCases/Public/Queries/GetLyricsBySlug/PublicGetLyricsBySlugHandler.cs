using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Handles the <see cref="PublicGetLyricsBySlugQuery" /> to serve a published lyrics page with
/// its navigation shell.
/// </summary>
/// <param name="lyricsRepository">Repository resolving the lyrics and the like state.</param>
/// <param name="pageService">Service assembling the page links.</param>
/// <param name="lyricsDtoService">Service assembling the public lyrics detail.</param>
public class PublicGetLyricsBySlugHandler(
    ILyricsRepository lyricsRepository,
    IPublicLyricsPageService pageService,
    ILyricsDtoService lyricsDtoService
) : IQueryHandler<PublicGetLyricsBySlugQuery, PublicGetLyricsBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsBySlugResult> Handle(
        PublicGetLyricsBySlugQuery query,
        CancellationToken cancellationToken = default
    )
    {
        LyricsEntity? lyrics = await lyricsRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (lyrics is null || lyrics.Status != EnumContentStatus.Published)
        {
            throw LyricsErrors.NotFound(id: Guid.Empty);
        }

        LyricsPageLinks links = await pageService.ResolveLinksAsync(
            lyrics: lyrics,
            cancellationToken: cancellationToken
        );

        bool isLiked =
            query.CurrentUserId is Guid currentUserId
            && await lyricsRepository.HasLikedAsync(
                userId: currentUserId,
                lyricsId: lyrics.Id,
                cancellationToken: cancellationToken
            );

        var dto = await lyricsDtoService.CreatePublicDetailAsync(lyrics, isLiked, cancellationToken);
        return new PublicGetLyricsBySlugResult(
            Lyrics: dto,
            VideoSlug: links.VideoSlug,
            ArtistSlug: links.ArtistSlug,
            AlbumTracks: links.AlbumTracks,
            StreamingLinks: links.StreamingLinks
        );
    }
}
