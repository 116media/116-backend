using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
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
            query = query.Where(category => category.IsActive == isActive.Value);
        }

        if (isFree.HasValue)
        {
            query = query.Where(category => category.IsFree == isFree.Value);
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
        return await Context.Categories.FirstOrDefaultAsync(
            category => EF.Functions.ILike(category.Slug, slug),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetActiveByContentTypeAsync(
        Guid? contentTypeId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<CategoryEntity> query = Context.Categories.Where(category => category.IsActive);

        if (contentTypeId.HasValue)
        {
            query = query.Where(category => category.ContentTypeId == contentTypeId.Value);
        }

        return await query.Include(c => c.ContentType).OrderBy(c => c.Name).ToListAsync(cancellationToken);
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
        return await Context
            .Categories.Where(category => category.IsGossip && category.IsActive)
            .Include(c => c.ContentType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetExclusiveCategoryAsync(CancellationToken cancellationToken = default)
    {
        return await Context
            .Categories.AsTracking()
            .Where(category => category.IsExclusive && category.IsActive)
            .Include(c => c.ContentType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetDefaultLyricsCategoryAsync(CancellationToken cancellationToken = default)
    {
        return await Context
            .Categories.AsTracking()
            .Where(category => category.IsDefaultForLyrics && category.IsActive)
            .Include(c => c.ContentType)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryEntity>> GetPinnedToFeedCategoriesAsync(
        Guid? contentTypeId = null,
        CancellationToken cancellationToken = default
    )
    {
        // Filters on PinnedToFeedAt (the mapped column), not the [NotMapped] IsPinnedToFeed
        // property, so it translates to SQL.
        IQueryable<CategoryEntity> query = Context
            .Categories.AsTracking()
            .Where(category => category.PinnedToFeedAt != null && category.IsActive);

        if (contentTypeId.HasValue)
        {
            query = query.Where(category => category.ContentTypeId == contentTypeId.Value);
        }

        return await query
            .Include(c => c.ContentType)
            .OrderByDescending(c => c.PinnedToFeedAt)
            .ToListAsync(cancellationToken);
    }
}
