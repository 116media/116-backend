using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Assembles the navigation shell of a lyrics page for the public lyrics-by-slug query.
/// </summary>
/// <param name="videoRepository">Repository resolving the linked video.</param>
/// <param name="artistRepository">Repository resolving the linked artist.</param>
/// <param name="albumRepository">Repository resolving the album.</param>
/// <param name="streamingLinkRepository">Repository resolving the curated streaming links.</param>
/// <param name="lyricsRepository">Repository resolving the album's sibling tracks.</param>
public class PublicLyricsPageService(
    IVideoRepository videoRepository,
    IArtistRepository artistRepository,
    IAlbumRepository albumRepository,
    IStreamingLinkRepository streamingLinkRepository,
    ILyricsRepository lyricsRepository
) : IPublicLyricsPageService
{
    /// <inheritdoc />
    public async Task<LyricsPageLinks> ResolveLinksAsync(LyricsEntity lyrics, CancellationToken cancellationToken)
    {
        string? videoSlug = null;

        if (lyrics.VideoId is { } videoId)
        {
            VideoEntity? video = await videoRepository.GetByIdAsync(id: videoId, cancellationToken: cancellationToken);
            videoSlug = video?.Slug.Value;
        }

        string? artistSlug = null;

        if (lyrics.ArtistId is { } artistId)
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(
                id: artistId,
                cancellationToken: cancellationToken
            );
            artistSlug = artist?.Slug.Value;
        }

        if (lyrics.AlbumId is not { } albumId)
        {
            IReadOnlyDictionary<EnumStreamingPlatform, string> singleCurated =
                await streamingLinkRepository.GetByLyricsAsync(
                    lyricsId: lyrics.Id,
                    cancellationToken: cancellationToken
                );

            return new LyricsPageLinks(
                VideoSlug: videoSlug,
                ArtistSlug: artistSlug,
                AlbumTracks: [],
                StreamingLinks: Links(lyrics.ArtistName, lyrics.SongTitle, singleCurated)
            );
        }

        AlbumEntity? album = await albumRepository.GetByIdAsync(id: albumId, cancellationToken: cancellationToken);
        List<LyricsEntity> siblingTracks = await lyricsRepository.GetPublishedByAlbumAsync(
            albumId: albumId,
            excludeLyricsId: lyrics.Id,
            cancellationToken: cancellationToken
        );
        IReadOnlyDictionary<EnumStreamingPlatform, string> curated = await streamingLinkRepository.GetByAlbumAsync(
            albumId: albumId,
            cancellationToken: cancellationToken
        );

        return new LyricsPageLinks(
            VideoSlug: videoSlug,
            ArtistSlug: artistSlug,
            AlbumTracks: siblingTracks
                .Select(track => new AlbumTrackDto(Slug: track.Slug, SongTitle: track.SongTitle))
                .ToList(),
            StreamingLinks: Links(lyrics.ArtistName, album?.Name ?? lyrics.SongTitle, curated)
        );
    }

    private static IReadOnlyList<StreamingLinkDto> Links(
        string artistName,
        string releaseName,
        IReadOnlyDictionary<EnumStreamingPlatform, string> curated
    )
    {
        return StreamingLinkFactory
            .CreateStreamingLinks(artistName: artistName, releaseName: releaseName, curated: curated)
            .Select(link => new StreamingLinkDto(Platform: link.Platform.ToString(), Url: link.Url))
            .ToList();
    }
}
