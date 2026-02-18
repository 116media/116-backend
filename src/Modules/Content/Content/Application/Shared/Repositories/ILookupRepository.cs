using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for all lookup-table entities in the Content module.
/// Provides methods for retrieving and persisting ContentType, PricingTier, PromotionLevel, and Tag entities.
/// </summary>
public interface ILookupRepository : IRepository<ContentTypeEntity>
{
    /// <summary>
    /// Adds a new content type entity to the repository.
    /// </summary>
    /// <param name="contentType">The content type entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddContentTypeAsync(ContentTypeEntity contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a content type by its unique identifier, throwing if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the content type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The content type entity if found.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">
    /// Thrown when no content type is found with the specified ID.
    /// </exception>
    Task<ContentTypeEntity> GetContentTypeByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a content type with the given name already exists.
    /// </summary>
    /// <param name="name">The content type name to check.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if a content type with the name exists, otherwise false.</returns>
    Task<bool> ContentTypeExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all content types ordered by name.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of all content type entities.</returns>
    Task<IReadOnlyList<ContentTypeEntity>> GetAllContentTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new pricing tier entity to the repository.
    /// </summary>
    /// <param name="pricingTier">The pricing tier entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddPricingTierAsync(PricingTierEntity pricingTier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a pricing tier by its unique identifier, throwing if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the pricing tier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The pricing tier entity if found.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">
    /// Thrown when no pricing tier is found with the specified ID.
    /// </exception>
    Task<PricingTierEntity> GetPricingTierByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a pricing tier with the given name already exists.
    /// </summary>
    /// <param name="name">The pricing tier name to check.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if a pricing tier with the name exists, otherwise false.</returns>
    Task<bool> PricingTierExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all pricing tiers ordered by name.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of all pricing tier entities.</returns>
    Task<IReadOnlyList<PricingTierEntity>> GetAllPricingTiersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new promotion level entity to the repository.
    /// </summary>
    /// <param name="promotionLevel">The promotion level entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddPromotionLevelAsync(PromotionLevelEntity promotionLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a promotion level by its unique identifier, throwing if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the promotion level.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The promotion level entity if found.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">
    /// Thrown when no promotion level is found with the specified ID.
    /// </exception>
    Task<PromotionLevelEntity> GetPromotionLevelByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks whether a promotion level with the given name already exists.
    /// </summary>
    /// <param name="name">The promotion level name to check.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if a promotion level with the name exists, otherwise false.</returns>
    Task<bool> PromotionLevelExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all promotion levels ordered by name.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of all promotion level entities.</returns>
    Task<IReadOnlyList<PromotionLevelEntity>> GetAllPromotionLevelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves only active promotion levels ordered by name.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of active promotion level entities.</returns>
    Task<IReadOnlyList<PromotionLevelEntity>> GetActivePromotionLevelsAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new tag entity to the repository.
    /// </summary>
    /// <param name="tag">The tag entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddTagAsync(TagEntity tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tag by its slug, or returns null if not found.
    /// </summary>
    /// <param name="slug">The URL-safe slug to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tag entity if found, otherwise null.</returns>
    Task<TagEntity?> GetTagBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tags ordered by name.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only list of all tag entities.</returns>
    Task<IReadOnlyList<TagEntity>> GetAllTagsAsync(CancellationToken cancellationToken = default);
}
