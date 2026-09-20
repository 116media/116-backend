using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for category and category pricing data access operations.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Retrieves a paginated list of categories with optional filters.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <param name="isFree">Optional filter by free/paid status.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of categories and the total count.</returns>
    Task<(List<CategoryEntity> Categories, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive,
        bool? isFree,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a category by its unique identifier, including its content type and pricing.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The category entity if found, otherwise null.</returns>
    Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a category by its unique identifier, including its content type and pricing.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the category.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The category entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the category is not found.</exception>
    Task<CategoryEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a category by its slug. Returns null if not found.
    /// </summary>
    /// <param name="slug">The URL-safe slug of the category.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The category entity if found, otherwise null.</returns>
    Task<CategoryEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active categories for a given content type.
    /// </summary>
    /// <param name="contentTypeId">The content type identifier to filter by, or null for all active categories.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of active category entities.</returns>
    Task<IReadOnlyList<CategoryEntity>> GetActiveByContentTypeAsync(
        Guid? contentTypeId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new category to the repository.
    /// </summary>
    Task AddAsync(CategoryEntity category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the single active category designated as the gossip fallback source for
    /// the homepage article promotion feed. Returns null if no such category is configured.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// The gossip fallback category entity if one exists, otherwise null.
    /// </returns>
    Task<CategoryEntity?> GetGossipCategoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the single active category currently marked as the exclusive show
    /// for the homepage. Returns null if no category is currently exclusive.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// The exclusive category entity if one exists, otherwise null.
    /// </returns>
    Task<CategoryEntity?> GetExclusiveCategoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the single active category designated as the default category for lyrics
    /// pages. Returns null if no such category is configured.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// The default lyrics category entity if one exists, otherwise null.
    /// </returns>
    Task<CategoryEntity?> GetDefaultLyricsCategoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active categories currently pinned to the content feed, optionally
    /// filtered to a single content type, ordered by PinnedToFeedAt descending (most recently
    /// pinned first). Used by the public feed query and by the admin pin handler to enforce
    /// the per-content-type cap and pick the FIFO eviction victim.
    /// </summary>
    /// <param name="contentTypeId">Optional content type filter, or null for all pinned categories.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of pinned category entities, newest first.</returns>
    Task<IReadOnlyList<CategoryEntity>> GetPinnedToFeedCategoriesAsync(
        Guid? contentTypeId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stages a modified category for the next commit. The write is explicit so it
    /// does not depend on the change tracker having observed the mutation.
    /// </summary>
    /// <param name="category">The modified category.</param>
    void Update(CategoryEntity category);

    /// <summary>
    /// Resolves the categorys the given ids reference, in one query, keyed by id.
    /// Missing ids are simply absent from the result.
    /// </summary>
    /// <param name="ids">The identifiers to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<IReadOnlyDictionary<Guid, CategoryEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    );
}
