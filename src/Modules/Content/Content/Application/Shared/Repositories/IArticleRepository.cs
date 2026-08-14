using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

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
public interface IArticleRepository : IRepository<ArticleEntity>
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
    /// Returns true if the user has already liked the given article.
    /// </summary>
    Task<bool> HasLikedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a like record to the repository.
    /// </summary>
    Task AddLikeAsync(ArticleLikeEntity like, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the like record for the given user and article.
    /// </summary>
    Task RemoveLikeAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has already bookmarked the given article.
    /// </summary>
    Task<bool> HasBookmarkedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a bookmark record to the repository.
    /// </summary>
    Task AddBookmarkAsync(ArticleBookmarkEntity bookmark, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the bookmark record for the given user and article.
    /// </summary>
    Task RemoveBookmarkAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the subsets of the given article ids that the specified user has liked and
    /// bookmarked, used to stamp the per-user interaction flags on article DTOs. Executes
    /// one query per interaction type; returns empty sets for an anonymous caller
    /// (<paramref name="currentUserId" /> null) or an empty id list, running no queries
    /// in that case.
    /// </summary>
    /// <param name="currentUserId">The authenticated caller's id, or null when anonymous.</param>
    /// <param name="articleIds">The candidate article ids, typically one page of a feed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The liked and bookmarked id sets for the current user.</returns>
    Task<(IReadOnlySet<Guid> Liked, IReadOnlySet<Guid> Bookmarked)> GetLikedAndBookmarkedIdsAsync(
        Guid? currentUserId,
        IReadOnlyCollection<Guid> articleIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a share record to the repository.
    /// </summary>
    Task AddShareAsync(ArticleShareEntity share, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment record to the repository.
    /// </summary>
    Task AddCommentAsync(ArticleCommentEntity comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of top-level comments for an article (replies excluded),
    /// along with the total top-level count. Soft-deleted comments are included with a null body.
    /// </summary>
    Task<(List<ArticleCommentEntity> Comments, int TotalCount)> GetCommentsAsync(
        Guid articleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns a paginated list of non-deleted replies to a comment, along with total count,
    /// ordered by creation time ascending.
    /// </summary>
    /// <param name="parentCommentId">The parent (top-level) comment identifier.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of replies per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The page of replies and the total non-deleted reply count.</returns>
    Task<(List<ArticleCommentEntity> Replies, int TotalCount)> GetRepliesAsync(
        Guid parentCommentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the number of non-deleted direct replies for each of the given parent comment ids.
    /// Parents with no replies are absent from the result.
    /// </summary>
    /// <param name="parentCommentIds">The parent comment ids to count replies for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A map of parent comment id to its non-deleted reply count.</returns>
    Task<IReadOnlyDictionary<Guid, int>> GetReplyCountsAsync(
        IReadOnlyCollection<Guid> parentCommentIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns a single comment by its ID, or null if not found.
    /// </summary>
    Task<ArticleCommentEntity?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing comment as modified.
    /// </summary>
    void UpdateComment(ArticleCommentEntity comment);

    /// <summary>
    /// Returns true if the user has already liked the given comment.
    /// </summary>
    Task<bool> HasLikedCommentAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment-like record to the repository.
    /// </summary>
    Task AddCommentLikeAsync(ArticleCommentLikeEntity like, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the comment-like record for the given user and comment, if present.
    /// </summary>
    Task RemoveCommentLikeAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the subset of the given comment ids that the viewer has liked. Executes as a
    /// single query; ids the viewer has not liked are absent from the result.
    /// </summary>
    /// <param name="viewerUserId">The current viewer's user id.</param>
    /// <param name="commentIds">The candidate comment ids, typically one page of comments.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The distinct set of input ids the viewer has liked.</returns>
    Task<IReadOnlySet<Guid>> GetLikedCommentIdsAsync(
        Guid viewerUserId,
        IReadOnlyCollection<Guid> commentIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns a paginated list of articles bookmarked by the given user, along with total count.
    /// </summary>
    Task<(List<BookmarkedArticleActivity> Activities, int TotalCount)> GetBookmarkedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns published articles on which the user has at least one non-deleted comment or reply.
    /// </summary>
    Task<(List<CommentedArticleActivity> Activities, int TotalCount)> GetCommentedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns published articles currently liked by the user.
    /// </summary>
    Task<(List<ArticleActivity> Activities, int TotalCount)> GetLikedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns published articles shared by the user, grouped by article.
    /// </summary>
    Task<(List<ArticleActivity> Activities, int TotalCount)> GetSharedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the user's non-deleted comments and replies for one published article.
    /// </summary>
    Task<(List<ArticleCommentEntity> Comments, int TotalCount)> GetOwnCommentsForArticleAsync(
        Guid userId,
        Guid articleId,
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
}
