using _116.Content.Application.Lookup.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
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
        var specification = new PromotionLevelByNameSpecification(name: name);
        return await Context.PromotionLevels.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromotionLevelEntity>> GetAllAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<PromotionLevelEntity> query = string.IsNullOrWhiteSpace(search)
            ? Context.PromotionLevels
            : Context.PromotionLevels.ApplySpecification(new PromotionLevelSearchSpecification(search: search));

        return await query
            .OrderBy(x => x.Name)
            .Take(ContentConstants.MaxReferenceListSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromotionLevelEntity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var specification = new ActivePromotionLevelSpecification();
        return await Context
            .PromotionLevels.ApplySpecification(specification: specification)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, PromotionLevelEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    )
    {
        List<PromotionLevelEntity> entities = await Context
            .PromotionLevels.Where(entity => ids.Contains(entity.Id))
            .ToListAsync(cancellationToken);

        return entities.ToDictionary(entity => entity.Id);
    }
}
