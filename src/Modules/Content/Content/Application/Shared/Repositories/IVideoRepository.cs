using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository projection for one of the authenticated user's current video ratings.
/// </summary>
public sealed record RatedVideoActivity(VideoEntity Video, short Stars, DateTimeOffset LastInteractedAt);

/// <summary>
/// Repository projection for the authenticated user's grouped shares of one video.
/// </summary>
public sealed record SharedVideoActivity(
    VideoEntity Video,
    int ShareCount,
    DateTimeOffset LastInteractedAt,
    EnumShareChannel? LastShareChannel
);

/// <summary>
/// Repository interface for video data access operations.
/// </summary>
public interface IVideoRepository
{
    /// <summary>
    /// Retrieves a paginated list of videos with optional filters.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter videos by title, description, or meta fields.</param>
    /// <param name="status">Optional filter by content status.</param>
    /// <param name="categoryId">Optional filter by category identifier.</param>
    /// <param name="tagSlug">Optional filter by tag slug.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of videos and the total count.</returns>
    Task<(List<VideoEntity> Videos, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        string? tagSlug = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all active videos (excluding Archived and Rejected) without pagination.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A list of active video entities ordered by most recent first.</returns>
    Task<List<VideoEntity>> GetActiveAsync(CancellationToken cancellationToken = default);

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
    /// Retrieves all currently promoted published videos.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of promoted video entities.</returns>
    Task<IReadOnlyList<VideoEntity>> GetPromotedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves published videos ranked by a weighted engagement score (rating volume
    /// weighted by rating quality, plus shares), tie-broken by publish date descending.
    /// </summary>
    /// <param name="limit">Maximum number of videos to return.</param>
    /// <param name="categoryId">Optional category filter; when supplied, only videos in that category are ranked.</param>
    /// <param name="excludeId">Optional video id to omit, e.g. the video currently being viewed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of the most popular published video entities.</returns>
    Task<IReadOnlyList<VideoEntity>> GetPopularVideosAsync(
        int limit,
        Guid? categoryId,
        Guid? excludeId,
        CancellationToken cancellationToken = default
    );

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
    /// Returns the user's existing rating for a video, or null if none exists.
    /// </summary>
    Task<VideoRatingEntity?> GetRatingAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a rating record to the repository.
    /// </summary>
    Task AddRatingAsync(VideoRatingEntity rating, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all ratings for a given video (used for average/count recomputation).
    /// </summary>
    Task<List<VideoRatingEntity>> GetAllRatingsForVideoAsync(
        Guid videoId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a share record to the repository.
    /// </summary>
    Task AddShareAsync(VideoShareEntity share, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the authenticated user's current ratings of published videos, newest interaction first.
    /// </summary>
    Task<(IReadOnlyList<RatedVideoActivity> Activities, int TotalCount)> GetRatedVideosByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns published videos shared by the authenticated user, grouped by video.
    /// Anonymous and other users' share events are excluded.
    /// </summary>
    Task<(IReadOnlyList<SharedVideoActivity> Activities, int TotalCount)> GetSharedVideosByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all currently active promoted published videos assigned to the given
    /// <paramref name="spotPriority" /> via their linked promotion level.
    /// </summary>
    /// <param name="spotPriority">The spot priority (1, 2, or 3) to filter by.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// A read-only list of promoted video entities for the specified spot.
    /// </returns>
    Task<IReadOnlyList<VideoEntity>> GetActivePromotedBySpotAsync(
        int spotPriority,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves published free videos (no associated customer), excluding IDs already used
    /// elsewhere on the feed. Results are returned in an arbitrary order so the caller can
    /// apply an in-memory random shuffle.
    /// </summary>
    /// <param name="limit">The maximum number of videos to return.</param>
    /// <param name="excludeIds">Video identifiers to exclude from the result set.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// A read-only list of free video entities.
    /// </returns>
    Task<IReadOnlyList<VideoEntity>> GetFreeVideosAsync(
        int limit,
        IEnumerable<Guid> excludeIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the latest published videos for a single category, newest first.
    /// Ordered by PublishedAt descending, falling back to CreatedAt for rows with a
    /// null PublishedAt. Used to populate a category section in the content feed.
    /// </summary>
    /// <param name="categoryId">The category to fetch videos for.</param>
    /// <param name="limit">The maximum number of videos to return.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of published video entities, newest first.</returns>
    Task<IReadOnlyList<VideoEntity>> GetLatestPublishedByCategoryAsync(
        Guid categoryId,
        int limit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the published videos belonging to a single category.
    /// Used by the pin handler to enforce the minimum-videos eligibility gate.
    /// </summary>
    /// <param name="categoryId">The category to count videos for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The number of published videos in the category.</returns>
    Task<int> CountPublishedByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of published videos linked to a specific artist profile.
    /// Used to populate the public artist page's videos section.
    /// </summary>
    /// <param name="artistId">The artist profile to fetch videos for.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of published videos and the total count.</returns>
    Task<(List<VideoEntity> Videos, int TotalCount)> GetPublishedByArtistAsync(
        Guid artistId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Throws when no video row exists, without materializing the aggregate.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ExistsOrThrowAsync(Guid videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a signed delta to one engagement counter in a single statement, clamped at zero.
    /// Set-based by design: two concurrent likes cannot both read the same value and write the
    /// same increment, and the change tracker never sees the row, so the audit columns keep
    /// whatever the last editorial write set them to.
    /// </summary>
    /// <param name="videoId">The video whose counter moves.</param>
    /// <param name="kind">The engagement whose counter to move.</param>
    /// <param name="delta">The signed amount to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// Rows updated, <c>0</c> when the row no longer exists, or <c>null</c> when this entity
    /// carries no counter for the kind — a normal event, not a missing row.
    /// </returns>
    Task<int?> ApplyEngagementDeltaAsync(
        Guid videoId,
        EnumEngagementKind kind,
        int delta,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets a video's cached rating average and count in a single statement. The average is a
    /// recomputed value rather than a delta, so it is set outright — but it is still written
    /// set-based so the change tracker leaves the editorial audit columns alone.
    /// </summary>
    /// <param name="videoId">The video whose rating is refreshed.</param>
    /// <param name="average">The recomputed average, already rounded.</param>
    /// <param name="count">The number of ratings the average was computed from.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>Rows updated; <c>0</c> when the video no longer exists.</returns>
    Task<int> SetRatingAsync(Guid videoId, decimal average, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether a published lyrics page is linked to the video — the one place the
    /// fact is computed from its source of truth.
    /// </summary>
    /// <param name="videoId">The video identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<bool> HasPublishedLyricsAsync(Guid videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the subset of the given video ids that have a published lyrics page linked,
    /// in one query — the batch companion of <see cref="HasPublishedLyricsAsync" />.
    /// </summary>
    /// <param name="videoIds">The video identifiers to probe.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<IReadOnlySet<Guid>> GetIdsWithPublishedLyricsAsync(
        IReadOnlyCollection<Guid> videoIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resolves the videos the given ids reference, in one query, keyed by id.
    /// Missing ids are simply absent from the result.
    /// </summary>
    /// <param name="ids">The identifiers to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<IReadOnlyDictionary<Guid, VideoEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resolves the published videos among the given ids, in one query, keyed by id. An id whose
    /// video is unpublished or deleted is simply absent — which is how a playlist hides it.
    /// </summary>
    /// <param name="ids">The identifiers to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<IReadOnlyDictionary<Guid, VideoEntity>> GetPublishedByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    );
}
