using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Interactions.Factories;

/// <summary>
/// Factory implementation building playlist projections from a pre-resolved thumbnail map.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class PlaylistDtoFactory(IMapper mapper, IFileStorageService fileStorage) : IPlaylistDtoFactory
{
    /// <inheritdoc />
    public async Task<PlaylistDetailDto> CreateDetailAsync(PlaylistEntity playlist, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, string> thumbnailUrls = await ResolveThumbnailsAsync(
            playlist.DetailThumbnailFileIds(),
            ct
        );

        return playlist.ToPlaylistDetailDto(mapper, thumbnailUrls);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlaylistDto>> CreateManyAsync(
        IReadOnlyList<PlaylistEntity> playlists,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, string> thumbnailUrls = await ResolveThumbnailsAsync(
            playlists.SummaryThumbnailFileIds(),
            ct
        );

        return playlists.Select(playlist => playlist.ToPlaylistDto(mapper, thumbnailUrls)).ToList();
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
