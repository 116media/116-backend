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
    /// <param name="status">Optional filter by content status.</param>
    /// <param name="categoryId">Optional filter by category identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of articles and the total count.</returns>
    Task<(List<ArticleEntity> Articles, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
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
        DateTimeOffset cutoff,
        CancellationToken cancellationToken = default
    );

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
}
