using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

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
    /// Retrieves all currently featured published articles.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of featured article entities.</returns>
    Task<IReadOnlyList<ArticleEntity>> GetFeaturedAsync(CancellationToken cancellationToken = default);

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
    /// Adds a share record to the repository.
    /// </summary>
    Task AddShareAsync(ArticleShareEntity share, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment record to the repository.
    /// </summary>
    Task AddCommentAsync(ArticleCommentEntity comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of non-deleted comments for an article, along with total count.
    /// </summary>
    Task<(List<ArticleCommentEntity> Comments, int TotalCount)> GetCommentsAsync(
        Guid articleId,
        int page,
        int pageSize,
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
    /// Returns a paginated list of articles bookmarked by the given user, along with total count.
    /// </summary>
    Task<(List<ArticleEntity> Articles, int TotalCount)> GetBookmarkedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}
