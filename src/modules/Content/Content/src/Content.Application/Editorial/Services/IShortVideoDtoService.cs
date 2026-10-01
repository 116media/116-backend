using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Assembles short video response DTOs, resolving the file URLs, the parent video and, where the
/// wire shape carries it, the author profile.
/// </summary>
public interface IShortVideoDtoService
{
    /// <summary>
    /// Builds the admin DTO of one short without its author profile.
    /// </summary>
    /// <param name="shortVideo">The short.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<ShortVideoDto> CreateAsync(ShortVideoEntity shortVideo, CancellationToken ct = default);

    /// <summary>
    /// Builds the admin DTO of one short with its author profile.
    /// </summary>
    /// <param name="shortVideo">The short.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<ShortVideoDto> CreateWithAuthorAsync(ShortVideoEntity shortVideo, CancellationToken ct = default);

    /// <summary>
    /// Builds the admin DTOs of a list of shorts with their author profiles.
    /// </summary>
    /// <param name="shortVideos">The shorts.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<IReadOnlyList<ShortVideoDto>> CreateManyWithAuthorAsync(
        IReadOnlyList<ShortVideoEntity> shortVideos,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the public DTO of one short.
    /// </summary>
    /// <param name="shortVideo">The short.</param>
    /// <param name="isLiked">Whether the current user has liked it.</param>
    /// <param name="isBookmarked">Whether the current user has bookmarked it.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<PublicShortVideoDto> CreatePublicAsync(
        ShortVideoEntity shortVideo,
        bool isLiked = false,
        bool isBookmarked = false,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the public DTOs of a list of shorts.
    /// </summary>
    /// <param name="shortVideos">The shorts.</param>
    /// <param name="likedShortVideoIds">The shorts the current user liked.</param>
    /// <param name="bookmarkedShortVideoIds">The shorts the current user bookmarked.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<IReadOnlyList<PublicShortVideoDto>> CreatePublicManyAsync(
        IReadOnlyList<ShortVideoEntity> shortVideos,
        IReadOnlySet<Guid> likedShortVideoIds,
        IReadOnlySet<Guid> bookmarkedShortVideoIds,
        CancellationToken ct = default
    );
}
