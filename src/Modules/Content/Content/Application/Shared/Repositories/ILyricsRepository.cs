using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for lyrics data access operations.
/// </summary>
public interface ILyricsRepository : IRepository<LyricsEntity>
{
    /// <summary>
    /// Retrieves a paginated list of lyrics with optional filters.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter lyrics by song title, artist name, or lyrics text.</param>
    /// <param name="status">Optional filter by content status.</param>
    /// <param name="categoryId">Optional filter by category identifier.</param>
    /// <param name="language">Optional filter by ISO 639-1 language code.</param>
    /// <param name="sort">
    /// Optional sort order: <c>"newest"</c> (also the implicit default) sorts by
    /// <c>CreatedAt</c> descending; <c>"views"</c>/<c>"likes"</c>/<c>"shares"</c> sort by the
    /// matching interaction counter descending, tie-broken by <c>CreatedAt</c>.
    /// </param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of lyrics and the total count.</returns>
    Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        string? language = null,
        string? sort = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a lyrics page by its URL slug. Returns null if not found.
    /// </summary>
    /// <param name="slug">The URL-safe slug of the lyrics page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyrics entity if found, otherwise null.</returns>
    Task<LyricsEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyric's record by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the lyrics.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyric's entity if found, otherwise null.</returns>
    Task<LyricsEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyric's record by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the lyrics.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyric's entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the lyrics record is not found.</exception>
    Task<LyricsEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyrics record linked to the given video.
    /// Returns null if no lyrics are linked to the video.
    /// </summary>
    /// <param name="videoId">The video identifier to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyrics entity if found, otherwise null.</returns>
    Task<LyricsEntity?> GetByVideoIdAsync(Guid videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new lyrics record to the repository.
    /// </summary>
    Task AddAsync(LyricsEntity lyrics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing lyrics record as modified.
    /// </summary>
    void Update(LyricsEntity lyrics);

    /// <summary>
    /// Marks a lyrics record for deletion from the repository.
    /// </summary>
    void Remove(LyricsEntity lyrics);

    /// <summary>
    /// Replaces the full set of tags applied to a lyrics page. Removes every existing
    /// <see cref="LyricsTagEntity" /> row for the given lyrics id and inserts the new set in
    /// one call. An empty <paramref name="tagIds" /> collection clears all tags.
    /// </summary>
    /// <param name="lyricsId">The lyrics page whose tag set is being replaced.</param>
    /// <param name="tagIds">The complete new set of tag identifiers.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ReplaceTagsAsync(
        Guid lyricsId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated list of published lyrics pages linked to a specific artist profile.
    /// Used to populate the public artist page's lyrics section.
    /// </summary>
    /// <param name="artistId">The artist profile to fetch lyrics for.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of published lyrics and the total count.</returns>
    Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetPublishedByArtistAsync(
        Guid artistId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves every other published lyrics page belonging to the same album, excluding the
    /// current lyrics page itself. Used to populate the "more from this album" section on the
    /// public lyrics detail page. Not paginated — album track counts are small.
    /// </summary>
    /// <param name="albumId">The album to fetch sibling tracks for.</param>
    /// <param name="excludeLyricsId">The current lyrics page id, excluded from the results.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The list of other published lyrics pages on the same album, oldest first.</returns>
    Task<List<LyricsEntity>> GetPublishedByAlbumAsync(
        Guid albumId,
        Guid excludeLyricsId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the lyrics page fulfilling a given order item. Returns null if no lyrics page
    /// is linked to the order item (e.g. the item fulfils an article or video instead).
    /// </summary>
    /// <param name="orderItemId">The order item identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyrics entity if found, otherwise null.</returns>
    Task<LyricsEntity?> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has already liked the given lyrics page.
    /// </summary>
    /// <param name="userId">The identity user UUID of the user.</param>
    /// <param name="lyricsId">The lyrics page to check.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<bool> HasLikedAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a like record to the repository.
    /// </summary>
    /// <param name="like">The like entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddLikeAsync(LyricsLikeEntity like, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the like record for the given user and lyrics page.
    /// </summary>
    /// <param name="userId">The identity user UUID of the user.</param>
    /// <param name="lyricsId">The lyrics page whose like is being removed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task RemoveLikeAsync(Guid userId, Guid lyricsId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a share record to the repository.
    /// </summary>
    /// <param name="share">The share entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddShareAsync(LyricsShareEntity share, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a raw view event record to the repository.
    /// </summary>
    /// <param name="viewEvent">The view event entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddViewEventAsync(LyricsViewEventEntity viewEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if a counted view event exists for the given lyrics page and dedup key
    /// created at or after the given instant.
    /// </summary>
    /// <param name="lyricsId">The lyrics page to check.</param>
    /// <param name="dedupKey">The identity surrogate the view is deduplicated against.</param>
    /// <param name="since">The start of the dedup window.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<bool> HasCountedViewSinceAsync(
        Guid lyricsId,
        string dedupKey,
        DateTime since,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the subset of the given lyrics ids that the specified user has liked, used to
    /// stamp the per-user <c>IsLiked</c> flag on lyrics DTOs. Returns an empty set for an
    /// anonymous caller (<paramref name="currentUserId" /> null) or an empty id list, running
    /// no query in that case.
    /// </summary>
    /// <param name="currentUserId">The authenticated caller's id, or null when anonymous.</param>
    /// <param name="lyricsIds">The candidate lyrics ids, typically one page of a feed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The liked id set for the current user.</returns>
    Task<IReadOnlySet<Guid>> GetLikedIdsAsync(
        Guid? currentUserId,
        IReadOnlyCollection<Guid> lyricsIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resolves the similar-lyrics waterfall for a given lyrics page (spec 06): published pages
    /// linked to a video in the same category (top 10 by recency); if that yields none, published
    /// pages sharing at least one tag (top 10 by shared-tag count desc, then recency); if that
    /// also yields none, the most recent standalone published pages (top 10 by recency). Each
    /// branch is tried in order regardless of whether the source page is video-linked — a
    /// video-linked page with no same-category matches still falls through to the shared-tags
    /// branch rather than stopping empty.
    /// </summary>
    /// <param name="lyricsId">The source lyrics page to find similar pages for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// The first non-empty branch's matches, or an empty list if none of the three branches
    /// yield any matches.
    /// </returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the source lyrics page is not found.</exception>
    Task<IReadOnlyList<LyricsEntity>> GetSimilarAsync(Guid lyricsId, CancellationToken cancellationToken = default);
}
