using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Playlist entity mappings.
/// </summary>
public static class PlaylistMapper
{
    /// <summary>
    /// Registers Playlist entity mappings into the provided TypeAdapterConfig.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<PlaylistVideoEntity, VideoInPlaylistDto>()
            .Map(dest => dest.VideoId, src => src.VideoId)
            .Map(dest => dest.Title, src => src.Video.Title)
            .Map(dest => dest.Slug, src => src.Video.Slug)
            .Map(dest => dest.CategoryName, src => src.Video.Category != null ? src.Video.Category.Name : string.Empty)
            .Map(dest => dest.ThumbnailUrl, _ => (string?)null)
            .Map(dest => dest.PublishedAt, src => src.Video.PublishedAt)
            .Map(dest => dest.ShareCount, src => src.Video.ShareCount)
            .Map(dest => dest.RatingAverage, src => src.Video.RatingAverage)
            .Map(dest => dest.RatingCount, src => src.Video.RatingCount)
            .Map(dest => dest.SortOrder, src => src.SortOrder);

        config
            .NewConfig<PlaylistEntity, PlaylistDto>()
            .Map(dest => dest.VideoCount, src => src.Videos.Count)
            .Map(dest => dest.ThumbnailUrls, _ => Array.Empty<string?>());

        config
            .NewConfig<PlaylistEntity, PlaylistDetailDto>()
            .Map(dest => dest.Videos, _ => Array.Empty<VideoInPlaylistDto>());
    }

    /// <summary>
    /// The distinct thumbnail files a detail projection needs, so the caller can resolve them all
    /// in one query before mapping.
    /// </summary>
    /// <param name="entity">The playlist to inspect.</param>
    /// <returns>The file ids.</returns>
    public static IReadOnlyCollection<Guid> DetailThumbnailFileIds(this PlaylistEntity entity)
    {
        return entity
            .Videos.OrderBy(video => video.SortOrder)
            .Select(playlistVideo => playlistVideo.Video?.ThumbnailFileId)
            .OfType<Guid>()
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// The distinct thumbnail files a summary projection needs — the first four videos of each
    /// playlist — so the caller can resolve them all in one query before mapping.
    /// </summary>
    /// <param name="entities">The playlists to inspect.</param>
    /// <returns>The file ids.</returns>
    public static IReadOnlyCollection<Guid> SummaryThumbnailFileIds(this IReadOnlyList<PlaylistEntity> entities)
    {
        return entities
            .SelectMany(playlist => playlist.Videos.OrderBy(video => video.SortOrder).Take(4))
            .Select(playlistVideo => playlistVideo.Video?.ThumbnailFileId)
            .OfType<Guid>()
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Maps a <see cref="PlaylistEntity" /> to a <see cref="PlaylistDetailDto" /> from already
    /// resolved thumbnail URLs. Performs no IO.
    /// </summary>
    /// <param name="entity">The playlist to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="thumbnailUrls">The resolved thumbnail URLs, keyed by file id.</param>
    /// <returns>The detail projection.</returns>
    public static PlaylistDetailDto ToPlaylistDetailDto(
        this PlaylistEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, string> thumbnailUrls
    )
    {
        var dto = mapper.Map<PlaylistDetailDto>(entity);

        IReadOnlyList<VideoInPlaylistDto> videos = entity
            .Videos.OrderBy(video => video.SortOrder)
            .Select(playlistVideo => new VideoInPlaylistDto(
                playlistVideo.VideoId,
                playlistVideo.Video.Title,
                playlistVideo.Video.Slug,
                playlistVideo.Video.Category?.Name ?? string.Empty,
                ThumbnailUrl(playlistVideo, thumbnailUrls),
                playlistVideo.Video.PublishedAt,
                playlistVideo.Video.ShareCount,
                playlistVideo.Video.RatingAverage,
                playlistVideo.Video.RatingCount,
                playlistVideo.SortOrder
            ))
            .ToList();

        return dto with
        {
            Videos = videos,
        };
    }

    /// <summary>
    /// Maps a <see cref="PlaylistEntity" /> to a <see cref="PlaylistDto" /> from already resolved
    /// thumbnail URLs, filling the cover strip from the first four videos. Performs no IO.
    /// </summary>
    /// <param name="entity">The playlist to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="thumbnailUrls">The resolved thumbnail URLs, keyed by file id.</param>
    /// <returns>The summary projection.</returns>
    public static PlaylistDto ToPlaylistDto(
        this PlaylistEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, string> thumbnailUrls
    )
    {
        var dto = mapper.Map<PlaylistDto>(entity);

        IReadOnlyList<string?> slots = entity
            .Videos.OrderBy(video => video.SortOrder)
            .Take(4)
            .Select(playlistVideo => ThumbnailUrl(playlistVideo, thumbnailUrls))
            .ToList();

        return dto with
        {
            VideoCount = entity.Videos.Count,
            ThumbnailUrls = slots,
        };
    }

    /// <summary>
    /// Reads a playlist entry's thumbnail URL out of the resolved map.
    /// </summary>
    /// <param name="playlistVideo">The playlist entry.</param>
    /// <param name="thumbnailUrls">The resolved thumbnail URLs, keyed by file id.</param>
    /// <returns>The URL, or null when the video has no thumbnail.</returns>
    private static string? ThumbnailUrl(
        PlaylistVideoEntity playlistVideo,
        IReadOnlyDictionary<Guid, string> thumbnailUrls
    )
    {
        return
            playlistVideo.Video?.ThumbnailFileId is { } fileId
            && thumbnailUrls.TryGetValue(fileId, out string? storageUrl)
            ? storageUrl
            : null;
    }
}
