using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository projection for a bookmarked article and its interaction timestamp.
/// </summary>
public sealed record BookmarkedArticleActivity(ArticleEntity Article, DateTimeOffset BookmarkedAt);

/// <summary>
/// Repository projection for grouped current-user comment activity on an article.
/// </summary>
public sealed record CommentedArticleActivity(
    ArticleEntity Article,
    ArticleCommentEntity LatestComment,
    int CommentCount,
    DateTimeOffset LastCommentedAt
);

/// <summary>
/// Repository projection for current-user like or grouped share activity on an article.
/// </summary>
public sealed record ArticleActivity(
    ArticleEntity Article,
    DateTimeOffset LastInteractedAt,
    int InteractionCount,
    EnumShareChannel? LastShareChannel = null
);

/// <summary>
/// Repository interface for article and article-image data access operations.
/// </summary>
public interface IArticleRepository
{
    /// <summary>
    /// Retrieves a paginated list of articles with optional filters.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter articles by title, description, or meta fields.</param>
    /// <param name="status">Optional filter by content status.</param>
    /// <param name="categoryId">Optional filter by category identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of articles and the total count.</returns>
    Task<(List<ArticleEntity> Articles, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves an article by its unique identifier, including related data.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The article entity if found, otherwise null.</returns>
    Task<ArticleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an article by its unique identifier, including related data.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The article entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the article is not found.</exception>
    Task<ArticleEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an article by its URL slug. Returns null if not found.
    /// </summary>
    /// <param name="slug">The URL-safe slug of the article.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The article entity if found, otherwise null.</returns>
    Task<ArticleEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all currently promoted published articles.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of promoted article entities.</returns>
    Task<IReadOnlyList<ArticleEntity>> GetPromotedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the most popular published articles, ranked by a weighted engagement score
    /// (like, comment, share, bookmark) and tie-broken by publish date descending.
    /// </summary>
    /// <param name="limit">Maximum number of articles to return.</param>
    /// <param name="categoryId">Optional category filter.</param>
    /// <param name="excludeId">Optional article id to omit from the result.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The ranked list of article entities.</returns>
    Task<IReadOnlyList<ArticleEntity>> GetPopularArticlesAsync(
        int limit,
        Guid? categoryId,
        Guid? excludeId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves draft articles with no content that were created before the specified cutoff date.
    /// Used by the background cleanup job to purge abandoned drafts.
    /// </summary>
    /// <param name="cutoff">Articles created before this date are considered abandoned.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of abandoned draft article entities.</returns>
    Task<IReadOnlyList<ArticleEntity>> GetAbandonedDraftsAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves an article linked to the given order item identifier. Returns null if not found.
    /// </summary>
    /// <param name="orderItemId">The order item identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<ArticleEntity?> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new article to the repository.
    /// </summary>
    Task AddAsync(ArticleEntity article, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing article as modified.
    /// </summary>
    void Update(ArticleEntity article);

    /// <summary>
    /// Marks an article for deletion from the repository.
    /// </summary>
    void Remove(ArticleEntity article);

    /// <summary>
    /// Adds a new article image record to the repository.
    /// </summary>
    Task AddImageAsync(ArticleImageEntity image, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all image records associated with a given article.
    /// </summary>
    /// <param name="articleId">The article identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of article image entities.</returns>
    Task<IReadOnlyList<ArticleImageEntity>> GetImagesByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Marks a collection of article image records for deletion.
    /// </summary>
    void RemoveImages(IEnumerable<ArticleImageEntity> images);

    /// <summary>
    /// Adds a new article-tag junction record to the repository.
    /// </summary>
    Task AddTagAsync(ArticleTagEntity tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an article-tag junction record for deletion.
    /// </summary>
    void RemoveTag(ArticleTagEntity tag);

    /// <summary>
    /// Retrieves all tag junction records for a given article.
    /// </summary>
    /// <param name="articleId">The article identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of article-tag junction entities.</returns>
    Task<IReadOnlyList<ArticleTagEntity>> GetTagsByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all artist junction records for a given article.
    /// </summary>
    /// <param name="articleId">The article identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of article-artist junction entities.</returns>
    Task<IReadOnlyList<ArticleArtistEntity>> GetArtistsByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Set-replaces an article's artist tags: removes junction rows not in the new list and
    /// adds rows not already present. Does not commit — the caller's unit of work owns the
    /// transaction, so the tag change stays atomic with the rest of the request.
    /// </summary>
    /// <param name="articleId">The article whose artist tags are being replaced.</param>
    /// <param name="artistIds">The complete new set of artist identifiers. Empty untags everything.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ReplaceArticleArtistsAsync(
        Guid articleId,
        IReadOnlyList<Guid> artistIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated page of published articles tagged to an artist, newest first.
    /// Shaped identically to the lyrics and video equivalents — it answers the same
    /// question for the third surface.
    /// </summary>
    /// <param name="artistId">The artist profile the articles are tagged to.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the page of articles and the total count.</returns>
    Task<(List<ArticleEntity> Articles, int TotalCount)> GetPublishedByArtistAsync(
        Guid artistId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all currently active promoted published articles assigned to the given
    /// <paramref name="spotPriority" /> via their linked promotion level.
    /// </summary>
    /// <param name="spotPriority">The spot priority (1, 2, or 3) to filter by.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// A read-only list of promoted article entities for the specified spot.
    /// </returns>
    Task<IReadOnlyList<ArticleEntity>> GetActivePromotedBySpotAsync(
        int spotPriority,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the most recent published articles from the gossip fallback category,
    /// ordered by <c>PublishedAt</c> descending, excluding any IDs already used elsewhere
    /// on the feed.
    /// </summary>
    /// <param name="gossipCategoryId">The identifier of the gossip fallback category.</param>
    /// <param name="limit">The maximum number of articles to return.</param>
    /// <param name="excludeIds">Article identifiers to exclude from the result set.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// A read-only list of gossip article entities ordered by publication date descending.
    /// </returns>
    Task<IReadOnlyList<ArticleEntity>> GetGossipFallbackAsync(
        Guid gossipCategoryId,
        int limit,
        IEnumerable<Guid> excludeIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Throws when no article row exists, without materializing the aggregate.
    /// </summary>
    /// <param name="articleId">The article identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ExistsOrThrowAsync(Guid articleId, CancellationToken cancellationToken = default);
}
