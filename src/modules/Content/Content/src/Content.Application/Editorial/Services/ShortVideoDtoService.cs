using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Identity.Contracts.Application.Services;
using _116.Storage.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Assembles short video response DTOs through the mapper extensions, owning the author profile,
/// the file URL and the parent video resolution.
/// </summary>
/// <param name="mapper">The Mapster mapper.</param>
/// <param name="userLookup">Service resolving author profiles from the Identity module.</param>
/// <param name="fileStorage">Storage contract resolving media and avatar URLs.</param>
/// <param name="videoRepository">Repository resolving the parent videos of teasers.</param>
public class ShortVideoDtoService(
    IMapper mapper,
    IUserLookupService userLookup,
    IFileStorageService fileStorage,
    IVideoRepository videoRepository
) : IShortVideoDtoService
{
    /// <inheritdoc />
    public Task<ShortVideoDto> CreateAsync(ShortVideoEntity shortVideo, CancellationToken ct = default)
    {
        return shortVideo.ToShortVideoDtoAsync(mapper, fileStorage, videoRepository, ct);
    }

    /// <inheritdoc />
    public Task<ShortVideoDto> CreateWithAuthorAsync(ShortVideoEntity shortVideo, CancellationToken ct = default)
    {
        return shortVideo.ToShortVideoDtoAsync(mapper, userLookup, fileStorage, videoRepository, ct);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ShortVideoDto>> CreateManyWithAuthorAsync(
        IReadOnlyList<ShortVideoEntity> shortVideos,
        CancellationToken ct = default
    )
    {
        return shortVideos.ToShortVideoDtosAsync(mapper, userLookup, fileStorage, videoRepository, ct);
    }

    /// <inheritdoc />
    public Task<PublicShortVideoDto> CreatePublicAsync(
        ShortVideoEntity shortVideo,
        bool isLiked = false,
        bool isBookmarked = false,
        CancellationToken ct = default
    )
    {
        return shortVideo.ToPublicShortVideoDtoAsync(
            mapper,
            userLookup,
            fileStorage,
            videoRepository,
            ct,
            isLiked: isLiked,
            isBookmarked: isBookmarked
        );
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PublicShortVideoDto>> CreatePublicManyAsync(
        IReadOnlyList<ShortVideoEntity> shortVideos,
        IReadOnlySet<Guid> likedShortVideoIds,
        IReadOnlySet<Guid> bookmarkedShortVideoIds,
        CancellationToken ct = default
    )
    {
        return shortVideos.ToPublicShortVideoDtosAsync(
            mapper,
            userLookup,
            fileStorage,
            videoRepository,
            likedShortVideoIds,
            bookmarkedShortVideoIds,
            ct
        );
    }
}
