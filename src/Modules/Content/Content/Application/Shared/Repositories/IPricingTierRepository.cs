using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing pricing-tier lookup entities.
/// </summary>
public interface IPricingTierRepository
{
    /// <summary>
    /// Stages a new pricing tier for insertion on the next commit.
    /// </summary>
    /// <param name="pricingTier">The pricing tier to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(PricingTierEntity pricingTier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tracked pricing tier by id for mutation.
    /// </summary>
    /// <param name="id">The pricing tier identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The tracked pricing tier.</returns>
    /// <exception cref="NotFoundException">Thrown when no pricing tier has the supplied id.</exception>
    Task<PricingTierEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether a pricing tier already carries the supplied name.
    /// </summary>
    /// <param name="name">The name to probe.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the name is taken.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads every pricing tier, optionally narrowed by a free-text search term.
    /// </summary>
    /// <param name="search">Free-text term, or null for the full list.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The pricing tiers ordered by name.</returns>
    Task<IReadOnlyList<PricingTierEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stages an existing pricing tier for update on the next commit.
    /// </summary>
    /// <param name="pricingTier">The pricing tier to update.</param>
    void Update(PricingTierEntity pricingTier);
}
