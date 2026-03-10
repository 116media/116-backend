using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for category and category pricing data access operations.
/// </summary>
public interface ICategoryRepository : IRepository<CategoryEntity>
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

    /// <summary>Adds a new category to the repository.</summary>
    Task AddAsync(CategoryEntity category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all pricing rows for a given category.
    /// </summary>
    /// <param name="categoryId">The identifier of the category.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of category pricing entities.</returns>
    Task<IReadOnlyList<CategoryPricingEntity>> GetPricingByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a specific pricing row by category and tier identifiers.
    /// Returns null if not found.
    /// </summary>
    Task<CategoryPricingEntity?> GetPricingAsync(
        Guid categoryId,
        Guid pricingTierId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Adds a new category pricing row to the repository.</summary>
    Task AddPricingAsync(CategoryPricingEntity pricing, CancellationToken cancellationToken = default);

    /// <summary>Removes a category pricing row from the repository.</summary>
    void RemovePricing(CategoryPricingEntity pricing);
}
