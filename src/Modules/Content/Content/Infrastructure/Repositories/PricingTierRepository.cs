using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
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
        return await Context.PricingTiers.AnyAsync(tier => EF.Functions.ILike(tier.Name, name), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PricingTierEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<PricingTierEntity> query = Context.PricingTiers;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(tier =>
                EF.Functions.ILike(tier.Name, pattern)
                || (tier.Description != null && EF.Functions.ILike(tier.Description, pattern))
            );
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }
}
