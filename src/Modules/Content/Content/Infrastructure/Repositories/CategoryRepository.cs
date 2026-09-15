using _116.Content.Application.Catalog.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ICategoryRepository" /> for managing category and category pricing entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class CategoryRepository(ContentDbContext context)
    : ContentRepository<CategoryEntity>(context),
        ICategoryRepository
{
    /// <inheritdoc />
    public async Task<(List<CategoryEntity> Categories, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive,
        bool? isFree,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<CategoryEntity> query = Context
            .Categories.Include(c => c.ContentType)
            .Include(c => c.Pricing)
                .ThenInclude(p => p.PricingTier);

        if (isActive.HasValue)
        {
            Specification<CategoryEntity> spec = isActive.Value
                ? new ActiveCategorySpecification()
                : new ActiveCategorySpecification().Not();
            query = query.ApplySpecification(specification: spec);
        }

        if (isFree.HasValue)
        {
            Specification<CategoryEntity> spec = isFree.Value
                ? new FreeCategorySpecification()
                : new PaidCategorySpecification();
            query = query.ApplySpecification(specification: spec);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<CategoryEntity> categories = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (categories, totalCount);
    }

    /// <inheritdoc />
    public override async Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context
            .Categories.Where(category => category.Id == id)
            .Include(c => c.ContentType)
            .Include(c => c.Pricing)
                .ThenInclude(p => p.PricingTier)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<CategoryEntity> GetByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .Categories.AsTracking()
            .Where(category => category.Id == id)
            .Include(c => c.ContentType)
            .Include(c => c.Pricing)
                .ThenInclude(p => p.PricingTier)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new CategoryBySlugSpecification(slug: slug);
        return await Context.Categories.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetActiveByContentTypeAsync(
        Guid? contentTypeId,
        CancellationToken cancellationToken = default
    )
    {
        Specification<CategoryEntity> spec = new ActiveCategorySpecification();

        if (contentTypeId.HasValue)
        {
            spec = spec.And(new CategoryByContentTypeSpecification(contentTypeId: contentTypeId.Value));
        }

        return await Context
            .Categories.ApplySpecification(specification: spec)
            .Include(c => c.ContentType)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryPricingEntity>> GetPricingByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .CategoryPricing.Where(pricing => pricing.CategoryId == categoryId)
            .Include(p => p.PricingTier)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryPricingEntity>> GetPricingByCategoriesAsync(
        IReadOnlyCollection<Guid> categoryIds,
        CancellationToken cancellationToken = default
    )
    {
        if (categoryIds.Count == 0)
        {
            return [];
        }

        return await Context
            .CategoryPricing.Where(pricing => categoryIds.Contains(pricing.CategoryId))
            .Include(p => p.PricingTier)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryPricingEntity?> GetPricingAsync(
        Guid categoryId,
        Guid pricingTierId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .CategoryPricing.AsTracking()
            .Where(pricing => pricing.CategoryId == categoryId && pricing.PricingTierId == pricingTierId)
            .Include(p => p.PricingTier)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddPricingAsync(CategoryPricingEntity pricing, CancellationToken cancellationToken = default)
    {
        await Context.CategoryPricing.AddAsync(pricing, cancellationToken);
    }

    /// <inheritdoc />
    public void RemovePricing(CategoryPricingEntity pricing)
    {
        Context.CategoryPricing.Remove(pricing);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetGossipCategoryAsync(CancellationToken cancellationToken = default)
    {
        var specification = new GossipCategorySpecification();
        return await Context
            .Categories.ApplySpecification(specification: specification)
            .Include(c => c.ContentType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetExclusiveCategoryAsync(CancellationToken cancellationToken = default)
    {
        var specification = new ExclusiveCategorySpecification();
        return await Context
            .Categories.AsTracking()
            .ApplySpecification(specification: specification)
            .Include(c => c.ContentType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetDefaultLyricsCategoryAsync(CancellationToken cancellationToken = default)
    {
        var specification = new DefaultLyricsCategorySpecification();
        return await Context
            .Categories.AsTracking()
            .ApplySpecification(specification: specification)
            .Include(c => c.ContentType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetPinnedToFeedCategoriesAsync(
        Guid? contentTypeId = null,
        CancellationToken cancellationToken = default
    )
    {
        Specification<CategoryEntity> specification = new PinnedToFeedCategorySpecification();

        if (contentTypeId.HasValue)
        {
            specification = specification.And(
                new CategoryByContentTypeSpecification(contentTypeId: contentTypeId.Value)
            );
        }

        return await Context
            .Categories.AsTracking()
            .ApplySpecification(specification: specification)
            .Include(c => c.ContentType)
            .OrderByDescending(c => c.PinnedToFeedAt)
            .ToListAsync(cancellationToken);
    }
}
