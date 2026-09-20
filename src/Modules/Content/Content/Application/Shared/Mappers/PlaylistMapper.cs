using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for Playlist entity mappings. A playlist row references its videos by
/// id, so every projection here takes the videos resolved by the caller and silently drops an
/// entry whose video is absent — which is how the published-only rule is applied.
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
            .NewConfig<PlaylistEntity, PlaylistDto>()
            .Map(dest => dest.VideoCount, src => src.Videos.Count)
            .Map(dest => dest.ThumbnailUrls, _ => Array.Empty<string?>());

        config
            .NewConfig<PlaylistEntity, PlaylistDetailDto>()
            .Map(dest => dest.Videos, _ => Array.Empty<VideoInPlaylistDto>());
    }

    /// <summary>
    /// The distinct video ids a playlist references, so the caller can resolve them in one query.
    /// </summary>
    /// <param name="entity">The playlist to inspect.</param>
    /// <returns>The video ids.</returns>
    public static IReadOnlyCollection<Guid> VideoIds(this PlaylistEntity entity)
    {
        return [.. entity.Videos.Select(playlistVideo => playlistVideo.VideoId).Distinct()];
    }

    /// <summary>
    /// The distinct video ids a set of playlists references, so the caller can resolve them in
    /// one query.
    /// </summary>
    /// <param name="entities">The playlists to inspect.</param>
    /// <returns>The video ids.</returns>
    public static IReadOnlyCollection<Guid> VideoIds(this IReadOnlyList<PlaylistEntity> entities)
    {
        return [.. entities.SelectMany(playlist => playlist.Videos).Select(video => video.VideoId).Distinct()];
    }

    /// <summary>
    /// The distinct thumbnail files a detail projection needs, so the caller can resolve them all
    /// in one query before mapping.
    /// </summary>
    /// <param name="entity">The playlist to inspect.</param>
    /// <param name="videos">The resolved videos, keyed by id.</param>
    /// <returns>The file ids.</returns>
    public static IReadOnlyCollection<Guid> DetailThumbnailFileIds(
        this PlaylistEntity entity,
        IReadOnlyDictionary<Guid, VideoEntity> videos
    )
    {
        return
        [
            .. entity
                .Videos.OrderBy(playlistVideo => playlistVideo.SortOrder)
                .Select(playlistVideo => videos.GetValueOrDefault(playlistVideo.VideoId)?.ThumbnailFileId)
                .OfType<Guid>()
                .Distinct(),
        ];
    }

    /// <summary>
    /// The distinct thumbnail files a summary projection needs — the first four videos of each
    /// playlist — so the caller can resolve them all in one query before mapping.
    /// </summary>
    /// <param name="entities">The playlists to inspect.</param>
    /// <param name="videos">The resolved videos, keyed by id.</param>
    /// <returns>The file ids.</returns>
    public static IReadOnlyCollection<Guid> SummaryThumbnailFileIds(
        this IReadOnlyList<PlaylistEntity> entities,
        IReadOnlyDictionary<Guid, VideoEntity> videos
    )
    {
        return
        [
            .. entities
                .SelectMany(playlist => playlist.VisibleVideos(videos).Take(4))
                .Select(playlistVideo => videos.GetValueOrDefault(playlistVideo.VideoId)?.ThumbnailFileId)
                .OfType<Guid>()
                .Distinct(),
        ];
    }

    /// <summary>
    /// Maps a <see cref="PlaylistEntity" /> to a <see cref="PlaylistDetailDto" /> from already
    /// resolved videos and thumbnail URLs. Performs no IO.
    /// </summary>
    /// <param name="entity">The playlist to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="videos">The resolved videos, keyed by id; an absent video drops its entry.</param>
    /// <param name="categories">The videos' categories, keyed by id.</param>
    /// <param name="thumbnailUrls">The resolved thumbnail URLs, keyed by file id.</param>
    /// <returns>The detail projection.</returns>
    public static PlaylistDetailDto ToPlaylistDetailDto(
        this PlaylistEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, VideoEntity> videos,
        IReadOnlyDictionary<Guid, CategoryEntity> categories,
        IReadOnlyDictionary<Guid, string> thumbnailUrls
    )
    {
        var dto = mapper.Map<PlaylistDetailDto>(entity);

        IReadOnlyList<VideoInPlaylistDto> entries =
        [
            .. entity
                .VisibleVideos(videos)
                .Select(playlistVideo => Project(playlistVideo, videos, categories, thumbnailUrls)),
        ];

        return dto with
        {
            Videos = entries,
        };
    }

    /// <summary>
    /// Maps a <see cref="PlaylistEntity" /> to a <see cref="PlaylistDto" /> from already resolved
    /// videos and thumbnail URLs, filling the cover strip from the first four. Performs no IO.
    /// </summary>
    /// <param name="entity">The playlist to map.</param>
    /// <param name="mapper">Injected IMapper instance.</param>
    /// <param name="videos">The resolved videos, keyed by id; an absent video drops its entry.</param>
    /// <param name="thumbnailUrls">The resolved thumbnail URLs, keyed by file id.</param>
    /// <returns>The summary projection.</returns>
    public static PlaylistDto ToPlaylistDto(
        this PlaylistEntity entity,
        IMapper mapper,
        IReadOnlyDictionary<Guid, VideoEntity> videos,
        IReadOnlyDictionary<Guid, string> thumbnailUrls
    )
    {
        var dto = mapper.Map<PlaylistDto>(entity);

        List<PlaylistVideoEntity> visible = [.. entity.VisibleVideos(videos)];
        IReadOnlyList<string?> slots =
        [
            .. visible.Take(4).Select(playlistVideo => ThumbnailUrl(playlistVideo, videos, thumbnailUrls)),
        ];

        return dto with
        {
            VideoCount = visible.Count,
            ThumbnailUrls = slots,
        };
    }

    /// <summary>
    /// The playlist's entries whose video was resolved, in sort order. An entry whose video is
    /// absent from the map — unpublished or deleted — is not shown.
    /// </summary>
    /// <param name="entity">The playlist to read.</param>
    /// <param name="videos">The resolved videos, keyed by id.</param>
    /// <returns>The visible entries, ordered.</returns>
    private static IEnumerable<PlaylistVideoEntity> VisibleVideos(
        this PlaylistEntity entity,
        IReadOnlyDictionary<Guid, VideoEntity> videos
    )
    {
        return entity
            .Videos.OrderBy(playlistVideo => playlistVideo.SortOrder)
            .Where(playlistVideo => videos.ContainsKey(playlistVideo.VideoId));
    }

    /// <summary>
    /// Projects one resolved playlist entry.
    /// </summary>
    /// <param name="playlistVideo">The playlist entry.</param>
    /// <param name="videos">The resolved videos, keyed by id.</param>
    /// <param name="categories">The videos' categories, keyed by id.</param>
    /// <param name="thumbnailUrls">The resolved thumbnail URLs, keyed by file id.</param>
    /// <returns>The entry projection.</returns>
    private static VideoInPlaylistDto Project(
        PlaylistVideoEntity playlistVideo,
        IReadOnlyDictionary<Guid, VideoEntity> videos,
        IReadOnlyDictionary<Guid, CategoryEntity> categories,
        IReadOnlyDictionary<Guid, string> thumbnailUrls
    )
    {
        VideoEntity video = videos[playlistVideo.VideoId];
        string categoryName = categories.TryGetValue(video.CategoryId, out CategoryEntity? category)
            ? category.Name
            : string.Empty;

        return new VideoInPlaylistDto(
            playlistVideo.VideoId,
            video.Title,
            video.Slug,
            categoryName,
            ThumbnailUrl(playlistVideo, videos, thumbnailUrls),
            video.PublishedAt,
            video.ShareCount,
            video.RatingAverage,
            video.RatingCount,
            playlistVideo.SortOrder
        );
    }

    /// <summary>
    /// Reads a playlist entry's thumbnail URL out of the resolved maps.
    /// </summary>
    /// <param name="playlistVideo">The playlist entry.</param>
    /// <param name="videos">The resolved videos, keyed by id.</param>
    /// <param name="thumbnailUrls">The resolved thumbnail URLs, keyed by file id.</param>
    /// <returns>The URL, or null when the video has no thumbnail.</returns>
    private static string? ThumbnailUrl(
        PlaylistVideoEntity playlistVideo,
        IReadOnlyDictionary<Guid, VideoEntity> videos,
        IReadOnlyDictionary<Guid, string> thumbnailUrls
    )
    {
        if (
            videos.GetValueOrDefault(playlistVideo.VideoId)?.ThumbnailFileId is not { } fileId
            || !thumbnailUrls.TryGetValue(fileId, out string? storageUrl)
        )
        {
            return null;
        }

        return storageUrl;
    }
}
