using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IPromotionLevelRepository" /> for managing promotion-level entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class PromotionLevelRepository(ContentDbContext context)
    : ContentRepository<PromotionLevelEntity>(context),
        IPromotionLevelRepository
{
    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await Context.PromotionLevels.AnyAsync(level => EF.Functions.ILike(level.Name, name), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromotionLevelEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<PromotionLevelEntity> query = Context.PromotionLevels;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(level => EF.Functions.ILike(level.Name, pattern));
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromotionLevelEntity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Context
            .PromotionLevels.Where(level => level.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
