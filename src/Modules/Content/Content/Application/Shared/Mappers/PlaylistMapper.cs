using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
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
    /// Maps a <see cref="PlaylistEntity" /> to a <see cref="PlaylistDetailDto" />,
    /// resolving video thumbnail URLs from associated FileEntity records.
    /// </summary>
    public static async Task<PlaylistDetailDto> ToPlaylistDetailDtoAsync(
        this PlaylistEntity entity,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        var dto = mapper.Map<PlaylistDetailDto>(entity);
        var orderedVideos = entity.Videos.OrderBy(v => v.SortOrder).ToList();
        IReadOnlyList<VideoInPlaylistDto> videoDtos = orderedVideos
            .Select(playlistVideo => new VideoInPlaylistDto(
                playlistVideo.VideoId,
                playlistVideo.Video.Title,
                playlistVideo.Video.Slug,
                playlistVideo.Video.Category?.Name ?? string.Empty,
                null,
                playlistVideo.Video.PublishedAt,
                playlistVideo.Video.RatingAverage,
                playlistVideo.Video.RatingCount,
                playlistVideo.SortOrder
            ))
            .ToList();

        Guid[] thumbnailFileIds = orderedVideos
            .Select(video => video.Video?.ThumbnailFileId)
            .OfType<Guid>()
            .Distinct()
            .ToArray();
        IReadOnlyDictionary<Guid, string> thumbnailUrls =
            thumbnailFileIds.Length == 0
                ? new Dictionary<Guid, string>()
                : await fileRepository.GetStorageUrlsByIdsAsync(thumbnailFileIds, ct);

        var resolved = new List<VideoInPlaylistDto>(videoDtos.Count);
        for (int i = 0; i < videoDtos.Count; i++)
        {
            VideoInPlaylistDto videoDto = videoDtos[i];
            PlaylistVideoEntity playlistVideo = orderedVideos[i];

            string? thumbnailUrl =
                playlistVideo.Video?.ThumbnailFileId is { } fileId
                && thumbnailUrls.TryGetValue(fileId, out string? storageUrl)
                    ? storageUrl
                    : null;

            resolved.Add(videoDto with { ThumbnailUrl = thumbnailUrl });
        }

        return dto with
        {
            Videos = resolved,
        };
    }

    /// <summary>
    /// Maps a list of <see cref="PlaylistEntity" /> to a list of <see cref="PlaylistDto" />.
    /// </summary>
    public static async Task<IReadOnlyList<PlaylistDto>> ToPlaylistDtosAsync(
        this IReadOnlyList<PlaylistEntity> entities,
        IMapper mapper,
        IFileRepository fileRepository,
        CancellationToken ct = default
    )
    {
        Guid[] thumbnailFileIds = entities
            .SelectMany(playlist => playlist.Videos.OrderBy(video => video.SortOrder).Take(4))
            .Select(playlistVideo => playlistVideo.Video?.ThumbnailFileId)
            .OfType<Guid>()
            .Distinct()
            .ToArray();
        IReadOnlyDictionary<Guid, string> thumbnailUrls =
            thumbnailFileIds.Length == 0
                ? new Dictionary<Guid, string>()
                : await fileRepository.GetStorageUrlsByIdsAsync(thumbnailFileIds, ct);

        return entities
            .Select(e =>
            {
                var dto = mapper.Map<PlaylistDto>(e);
                IReadOnlyList<string?> slots = e
                    .Videos.OrderBy(video => video.SortOrder)
                    .Take(4)
                    .Select(video =>
                        video.Video?.ThumbnailFileId is { } fileId
                        && thumbnailUrls.TryGetValue(fileId, out string? storageUrl)
                            ? storageUrl
                            : null
                    )
                    .ToList();

                return dto with
                {
                    VideoCount = e.Videos.Count,
                    ThumbnailUrls = slots,
                };
            })
            .ToList();
    }
}
