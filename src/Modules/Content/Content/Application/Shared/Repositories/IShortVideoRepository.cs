using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository projection for one short video in one of the authenticated user's activity
/// collections (liked, bookmarked, or shared).
/// </summary>
public sealed record ShortVideoActivity(
    ShortVideoEntity ShortVideo,
    DateTimeOffset LastInteractedAt,
    int InteractionCount = 1
);

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
    /// Retrieves one page of active short videos ordered by a per-session randomized shuffle
    /// (each row's stable <c>FeedRank</c> XOR the session seed), using keyset pagination so the
    /// ordering never drifts or repeats across pages.
    /// </summary>
    /// <param name="seed">The session shuffle seed; the same seed yields the same ordering.</param>
    /// <param name="afterSortKey">The last returned item's sort key, or null for the first page.</param>
    /// <param name="limit">The maximum number of short videos to return.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The ordered slice of active short videos after the cursor position.</returns>
    Task<IReadOnlyList<ShortVideoEntity>> GetRandomizedFeedAsync(
        long seed,
        long? afterSortKey,
        int limit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resolves which of the given short videos the user has liked and bookmarked, in one
    /// query per interaction type. Returns empty sets for anonymous callers or an empty id list.
    /// </summary>
    /// <param name="currentUserId">The requesting user id, or null when anonymous.</param>
    /// <param name="shortVideoIds">The short video ids to resolve interaction state for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The sets of liked and bookmarked short video ids.</returns>
    Task<(IReadOnlySet<Guid> Liked, IReadOnlySet<Guid> Bookmarked)> GetLikedAndBookmarkedIdsAsync(
        Guid? currentUserId,
        IReadOnlyCollection<Guid> shortVideoIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves active short videos liked by the user, newest like first with stable id ties.
    /// </summary>
    Task<(List<ShortVideoActivity> Items, int TotalCount)> GetLikedShortVideosAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves active short videos bookmarked by the user, newest bookmark first with stable id ties.
    /// </summary>
    Task<(List<ShortVideoActivity> Items, int TotalCount)> GetBookmarkedShortVideosAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves active short videos shared by the user, grouped by short and ordered by latest share.
    /// </summary>
    Task<(List<ShortVideoActivity> Items, int TotalCount)> GetSharedShortVideosAsync(
        Guid userId,
        int page,
        int pageSize,
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

    /// <summary>
    /// Adds a raw view event record to the repository.
    /// </summary>
    Task AddViewEventAsync(ShortVideoViewEventEntity viewEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a counted view event exists for the given short video and dedup key
    /// created at or after the given instant.
    /// </summary>
    Task<bool> HasCountedViewSinceAsync(
        Guid shortVideoId,
        string dedupKey,
        DateTime since,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes uncounted view events created before the given cutoff.
    /// Returns the number of rows removed.
    /// </summary>
    Task<int> PruneUncountedViewEventsAsync(DateTime cutoff, CancellationToken cancellationToken = default);
}
