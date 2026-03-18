using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for video data access operations.
/// </summary>
public interface IVideoRepository : IRepository<VideoEntity>
{
    /// <summary>
    /// Retrieves a paginated list of videos with optional filters.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter videos by title, description, or meta fields.</param>
    /// <param name="status">Optional filter by content status.</param>
    /// <param name="categoryId">Optional filter by category identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of videos and the total count.</returns>
    Task<(List<VideoEntity> Videos, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a video by its unique identifier, including related data.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the video.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The video entity if found, otherwise null.</returns>
    Task<VideoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a video by its unique identifier, including related data.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the video.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The video entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the video is not found.</exception>
    Task<VideoEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a video by its URL slug. Returns null if not found.
    /// </summary>
    /// <param name="slug">The URL-safe slug of the video.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The video entity if found, otherwise null.</returns>
    Task<VideoEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all currently featured published videos.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of featured video entities.</returns>
    Task<IReadOnlyList<VideoEntity>> GetFeaturedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a video linked to the given order item identifier. Returns null if not found.
    /// </summary>
    /// <param name="orderItemId">The order item identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<VideoEntity?> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new video to the repository.
    /// </summary>
    Task AddAsync(VideoEntity video, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing video as modified.
    /// </summary>
    void Update(VideoEntity video);

    /// <summary>
    /// Marks a video for deletion from the repository.
    /// </summary>
    void Remove(VideoEntity video);

    /// <summary>
    /// Adds a new video-tag junction record to the repository.
    /// </summary>
    Task AddTagAsync(VideoTagEntity tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a video-tag junction record for deletion.
    /// </summary>
    void RemoveTag(VideoTagEntity tag);

    /// <summary>
    /// Retrieves all tag junction records for a given video.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of video-tag junction entities.</returns>
    Task<IReadOnlyList<VideoTagEntity>> GetTagsByVideoIdAsync(
        Guid videoId,
        CancellationToken cancellationToken = default
    );
}
