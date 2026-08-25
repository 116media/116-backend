using _116.Content.Application.Lookup.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IPricingTierRepository" /> for managing pricing-tier entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class PricingTierRepository(ContentDbContext context)
    : ContentRepository<PricingTierEntity>(context),
        IPricingTierRepository
{
    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var specification = new PricingTierByNameSpecification(name: name);
        return await Context.PricingTiers.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PricingTierEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<PricingTierEntity> query = string.IsNullOrWhiteSpace(search)
            ? Context.PricingTiers
            : Context.PricingTiers.ApplySpecification(new PricingTierSearchSpecification(search: search));

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }
}
