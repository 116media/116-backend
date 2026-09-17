using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Editorial.Factories;

/// <summary>
/// Factory implementation building video projections from a pre-resolved thumbnail map.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class VideoDtoFactory(IMapper mapper, IFileStorageService fileStorage) : IVideoDtoFactory
{
    /// <inheritdoc />
    public async Task<VideoDetailDto> CreateDetailAsync(VideoEntity video, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync([video], ct);

        return video.ToVideoDetailDto(mapper, ThumbnailUrl(video, thumbnails));
    }

    /// <inheritdoc />
    public async Task<PublicVideoDetailDto> CreatePublicDetailAsync(
        VideoEntity video,
        short? ratedStars = null,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync([video], ct);

        return video.ToPublicVideoDetailDto(mapper, ThumbnailUrl(video, thumbnails), ratedStars);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoSummaryDto>> CreateManyAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync(videos, ct);

        return videos.Select(video => video.ToVideoSummaryDto(mapper, thumbnails)).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PublicVideoSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<VideoEntity> videos,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> thumbnails = await ResolveThumbnailsAsync(videos, ct);

        return videos.ToPublicVideoSummaryDtos(thumbnails);
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
