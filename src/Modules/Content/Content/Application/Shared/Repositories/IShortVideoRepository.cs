using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for short video data access operations.
/// </summary>
public interface IShortVideoRepository : IRepository<ShortVideoEntity>
{
    /// <summary>
    /// Retrieves a paginated list of short videos with an optional active filter.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter short videos by title.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of short videos and the total count.</returns>
    Task<(List<ShortVideoEntity> ShortVideos, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a short video by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the short video.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The short video entity if found, otherwise null.</returns>
    Task<ShortVideoEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a short video by its URL slug.
    /// Returns null if not found.
    /// </summary>
    /// <param name="slug">The URL slug of the short video.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The short video entity if found, otherwise null.</returns>
    Task<ShortVideoEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a short video by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the short video.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The short video entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the short video is not found.</exception>
    Task<ShortVideoEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new short video to the repository.
    /// </summary>
    Task AddAsync(ShortVideoEntity shortVideo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing short video as modified.
    /// </summary>
    void Update(ShortVideoEntity shortVideo);

    /// <summary>
    /// Marks a short video for deletion from the repository.
    /// </summary>
    void Remove(ShortVideoEntity shortVideo);

    /// <summary>
    /// Returns true if the user has already liked the given short video.
    /// </summary>
    Task<bool> HasLikedAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a like record to the repository.
    /// </summary>
    Task AddLikeAsync(ShortVideoLikeEntity like, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the like record for the given user and short video.
    /// </summary>
    Task RemoveLikeAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has already bookmarked the given short video.
    /// </summary>
    Task<bool> HasBookmarkedAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a bookmark record to the repository.
    /// </summary>
    Task AddBookmarkAsync(ShortVideoBookmarkEntity bookmark, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the bookmark record for the given user and short video.
    /// </summary>
    Task RemoveBookmarkAsync(Guid userId, Guid shortVideoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a share record to the repository.
    /// </summary>
    Task AddShareAsync(ShortVideoShareEntity share, CancellationToken cancellationToken = default);
}
