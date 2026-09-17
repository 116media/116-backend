using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Interactions.Factories;

/// <summary>
/// Factory implementation building playlist projections from pre-resolved videos and thumbnails.
/// Only published videos are resolved, so an entry whose video was unpublished or deleted drops
/// out of the projection — the rule the filtered include used to carry.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="videoRepository">Repository resolving the playlist's published videos.</param>
/// <param name="categoryRepository">Repository resolving those videos' categories.</param>
public class PlaylistDtoFactory(
    IMapper mapper,
    IFileStorageService fileStorage,
    IVideoRepository videoRepository,
    ICategoryRepository categoryRepository
) : IPlaylistDtoFactory
{
    /// <inheritdoc />
    public async Task<PlaylistDetailDto> CreateDetailAsync(PlaylistEntity playlist, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, VideoEntity> videos = await videoRepository.GetPublishedByIdsAsync(
            ids: playlist.VideoIds(),
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, CategoryEntity> categories = await categoryRepository.GetByIdsAsync(
            ids: [.. videos.Values.Select(video => video.CategoryId).Distinct()],
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, string> thumbnailUrls = await ResolveThumbnailsAsync(
            playlist.DetailThumbnailFileIds(videos),
            ct
        );

        return playlist.ToPlaylistDetailDto(mapper, videos, categories, thumbnailUrls);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlaylistDto>> CreateManyAsync(
        IReadOnlyList<PlaylistEntity> playlists,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, VideoEntity> videos = await videoRepository.GetPublishedByIdsAsync(
            ids: playlists.VideoIds(),
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, string> thumbnailUrls = await ResolveThumbnailsAsync(
            playlists.SummaryThumbnailFileIds(videos),
            ct
        );

        return [.. playlists.Select(playlist => playlist.ToPlaylistDto(mapper, videos, thumbnailUrls))];
    }

    /// <summary>
    /// Resolves the supplied thumbnail files, skipping the query when there are none.
    /// </summary>
    /// <param name="fileIds">The thumbnail files to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The URLs, keyed by file id.</returns>
    private async Task<IReadOnlyDictionary<Guid, string>> ResolveThumbnailsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken ct
    )
    {
        return fileIds.Count == 0 ? new Dictionary<Guid, string>() : await fileStorage.ResolveUrlsAsync(fileIds, ct);
    }
}
