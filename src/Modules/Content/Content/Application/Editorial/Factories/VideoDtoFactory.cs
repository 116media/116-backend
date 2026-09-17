using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Factory implementation building video projections from a pre-resolved thumbnail map
/// and the derived published-lyrics fact.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="videoRepository">Repository computing the published-lyrics fact.</param>
/// <param name="contentLookupFactory">Resolves the categories, customers and promotion levels named.</param>
public class VideoDtoFactory(
    IMapper mapper,
    IFileStorageService fileStorage,
    IVideoRepository videoRepository,
    IContentLookupFactory contentLookupFactory
) : IVideoDtoFactory
{
    /// <inheritdoc />
    public async Task<VideoDetailDto> CreateDetailAsync(VideoEntity video, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync([video], ct);
        bool hasLyrics = await videoRepository.HasPublishedLyricsAsync(videoId: video.Id, cancellationToken: ct);
        ContentLookups lookups = await contentLookupFactory.ResolveForVideosAsync([video], ct);

        return video.ToVideoDetailDto(mapper, lookups, ThumbnailUrl(video, thumbnails), hasLyrics);
    }

    /// <inheritdoc />
    public async Task<PublicVideoDetailDto> CreatePublicDetailAsync(
        VideoEntity video,
        short? ratedStars = null,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync([video], ct);
        bool hasLyrics = await videoRepository.HasPublishedLyricsAsync(videoId: video.Id, cancellationToken: ct);
        ContentLookups lookups = await contentLookupFactory.ResolveForVideosAsync([video], ct);

        return video.ToPublicVideoDetailDto(mapper, lookups, ThumbnailUrl(video, thumbnails), hasLyrics, ratedStars);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoSummaryDto>> CreateManyAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync(videos, ct);
        IReadOnlySet<Guid> videosWithLyrics = await ResolveVideosWithLyricsAsync(videos, ct);
        ContentLookups lookups = await contentLookupFactory.ResolveForVideosAsync(videos, ct);

        return
        [
            .. videos.Select(video =>
                video.ToVideoSummaryDto(mapper, lookups, thumbnails, videosWithLyrics.Contains(video.Id))
            ),
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PublicVideoSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync(videos, ct);
        IReadOnlySet<Guid> videosWithLyrics = await ResolveVideosWithLyricsAsync(videos, ct);
        ContentLookups lookups = await contentLookupFactory.ResolveForVideosAsync(videos, ct);

        return videos.ToPublicVideoSummaryDtos(lookups, thumbnails, videosWithLyrics);
    }

    /// <inheritdoc />
    public Task<IReadOnlySet<Guid>> ResolveVideosWithLyricsAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    )
    {
        return videoRepository.GetIdsWithPublishedLyricsAsync(
            videoIds: videos.Select(video => video.Id).ToList(),
            cancellationToken: ct
        );
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolveThumbnailsAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    )
    {
        return fileStorage.ResolveManyAsync(
            videos.Where(v => v.ThumbnailFileId.HasValue).Select(v => v.ThumbnailFileId!.Value).Distinct().ToList(),
            ct
        );
    }

    /// <summary>
    /// Reads a video's thumbnail URL out of the resolved map.
    /// </summary>
    /// <param name="video">The video.</param>
    /// <param name="thumbnails">The resolved thumbnails, keyed by file id.</param>
    /// <returns>The URL, or null when the video has no thumbnail.</returns>
    private static string? ThumbnailUrl(VideoEntity video, IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails) =>
        video.ThumbnailFileId.HasValue ? thumbnails.GetValueOrDefault(video.ThumbnailFileId.Value)?.StorageUrl : null;
}
