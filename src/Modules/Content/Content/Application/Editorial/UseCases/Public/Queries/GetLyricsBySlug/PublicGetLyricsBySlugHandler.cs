using _116.Content.Application.Shared;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Handles the <see cref="PublicGetLyricsBySlugQuery" /> to retrieve a lyrics page by its slug.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations, used to resolve the linked video's slug.</param>
/// <param name="artistRepository">Repository for artist profile data access operations, used to resolve the linked artist's slug.</param>
/// <param name="albumRepository">Repository for album data access operations, used to resolve the linked album's name and sibling tracks.</param>
/// <param name="streamingLinkRepository">Repository for resolving curated streaming platform links.</param>
/// <param name="mapper">The Mapster mapper used for tags.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileRepository">Repository for resolving avatar and cover image file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetLyricsBySlugHandler(
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    IArtistRepository artistRepository,
    IAlbumRepository albumRepository,
    IStreamingLinkRepository streamingLinkRepository,
    IMapper mapper,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    ContentI18n i18n
) : IQueryHandler<PublicGetLyricsBySlugQuery, PublicGetLyricsBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsBySlugResult> Handle(
        PublicGetLyricsBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity? lyrics = await lyricsRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (lyrics is null || lyrics.Status != EnumContentStatus.Published)
        {
            throw i18n.Lyrics.NotFound(id: Guid.Empty);
        }

        string? videoSlug = null;
        if (lyrics.VideoId is Guid videoId)
        {
            VideoEntity? video = await videoRepository.GetByIdAsync(id: videoId, cancellationToken: cancellationToken);
            videoSlug = video?.Slug;
        }

        string? artistSlug = null;
        if (lyrics.ArtistId is Guid artistId)
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(
                id: artistId,
                cancellationToken: cancellationToken
            );
            artistSlug = artist?.Slug;
        }

        IReadOnlyList<AlbumTrackDto> albumTracks;
        IReadOnlyList<StreamingLinkDto> streamingLinks;

        if (lyrics.AlbumId is Guid albumId)
        {
            AlbumEntity? album = await albumRepository.GetByIdAsync(id: albumId, cancellationToken: cancellationToken);

            List<LyricsEntity> siblingTracks = await lyricsRepository.GetPublishedByAlbumAsync(
                albumId: albumId,
                excludeLyricsId: lyrics.Id,
                cancellationToken: cancellationToken
            );
            albumTracks = siblingTracks
                .Select(track => new AlbumTrackDto(Slug: track.Slug, SongTitle: track.SongTitle))
                .ToList();

            IReadOnlyDictionary<EnumStreamingPlatform, string> curated = await streamingLinkRepository.GetByAlbumAsync(
                albumId: albumId,
                cancellationToken: cancellationToken
            );
            streamingLinks = StreamingLinkResolver
                .ResolveStreamingLinks(
                    artistName: lyrics.ArtistName,
                    releaseName: album?.Name ?? lyrics.SongTitle,
                    curated: curated
                )
                .Select(link => new StreamingLinkDto(Platform: link.Platform.ToString(), Url: link.Url))
                .ToList();
        }
        else
        {
            albumTracks = [];

            IReadOnlyDictionary<EnumStreamingPlatform, string> curated = await streamingLinkRepository.GetByLyricsAsync(
                lyricsId: lyrics.Id,
                cancellationToken: cancellationToken
            );
            streamingLinks = StreamingLinkResolver
                .ResolveStreamingLinks(artistName: lyrics.ArtistName, releaseName: lyrics.SongTitle, curated: curated)
                .Select(link => new StreamingLinkDto(Platform: link.Platform.ToString(), Url: link.Url))
                .ToList();
        }

        bool isLiked =
            query.CurrentUserId is Guid currentUserId
            && await lyricsRepository.HasLikedAsync(
                userId: currentUserId,
                lyricsId: lyrics.Id,
                cancellationToken: cancellationToken
            );

        var dto = await lyrics.ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository, cancellationToken, isLiked);
        return new PublicGetLyricsBySlugResult(
            Lyrics: dto,
            VideoSlug: videoSlug,
            ArtistSlug: artistSlug,
            AlbumTracks: albumTracks,
            StreamingLinks: streamingLinks
        );
    }
}
