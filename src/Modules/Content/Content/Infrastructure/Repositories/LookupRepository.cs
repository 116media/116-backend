using _116.Content.Application.Lookup.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ILookupRepository" /> for managing all lookup-table entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class LookupRepository(ContentDbContext context) : ILookupRepository
{
    /// <inheritdoc />
    public async Task AddContentTypeAsync(ContentTypeEntity contentType, CancellationToken cancellationToken = default)
    {
        await context.ContentTypes.AddAsync(contentType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ContentTypeEntity> GetContentTypeByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ContentTypeByIdSpecification(id: id);
        return await context
            .ContentTypes.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ContentTypeExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var specification = new ContentTypeByNameSpecification(name: name);
        return await context.ContentTypes.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContentTypeEntity>> GetAllContentTypesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context.ContentTypes.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddPricingTierAsync(PricingTierEntity pricingTier, CancellationToken cancellationToken = default)
    {
        await context.PricingTiers.AddAsync(pricingTier, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PricingTierEntity> GetPricingTierByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PricingTierByIdSpecification(id: id);
        return await context
            .PricingTiers.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> PricingTierExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var specification = new PricingTierByNameSpecification(name: name);
        return await context.PricingTiers.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PricingTierEntity>> GetAllPricingTiersAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context.PricingTiers.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddPromotionLevelAsync(
        PromotionLevelEntity promotionLevel,
        CancellationToken cancellationToken = default
    )
    {
        await context.PromotionLevels.AddAsync(promotionLevel, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PromotionLevelEntity> GetPromotionLevelByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PromotionLevelByIdSpecification(id: id);
        return await context
            .PromotionLevels.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> PromotionLevelExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var specification = new PromotionLevelByNameSpecification(name: name);
        return await context.PromotionLevels.AnyBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromotionLevelEntity>> GetAllPromotionLevelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context.PromotionLevels.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromotionLevelEntity>> GetActivePromotionLevelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ActivePromotionLevelSpecification();
        return await context
            .PromotionLevels.ApplySpecification(specification: specification)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddTagAsync(TagEntity tag, CancellationToken cancellationToken = default)
    {
        await context.Tags.AddAsync(tag, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TagEntity> GetTagByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new TagByIdSpecification(id: id);
        return await context
            .Tags.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(TagEntity entity)
    {
        context.Tags.Remove(entity);
    }

    /// <inheritdoc />
    public async Task<TagEntity?> GetTagBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new TagBySlugSpecification(slug: slug);
        return await context.Tags.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagEntity>> GetAllTagsAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TagEntity> query = string.IsNullOrWhiteSpace(search)
            ? context.Tags
            : context.Tags.ApplySpecification(new TagSearchSpecification(search: search));

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }
}
