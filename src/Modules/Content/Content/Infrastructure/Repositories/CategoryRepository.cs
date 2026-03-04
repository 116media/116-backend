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
public class CategoryRepository(ContentDbContext context) : ICategoryRepository
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
        IQueryable<CategoryEntity> query = context
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
    public async Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new CategoryByIdSpecification(id: id);
        return await context
            .Categories.ApplySpecification(specification: specification)
            .Include(c => c.ContentType)
            .Include(c => c.Pricing)
                .ThenInclude(p => p.PricingTier)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new CategoryByIdSpecification(id: id);
        return await context
            .Categories.ApplySpecification(specification: specification)
            .Include(c => c.ContentType)
            .Include(c => c.Pricing)
                .ThenInclude(p => p.PricingTier)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new CategoryBySlugSpecification(slug: slug);
        return await context.Categories.FirstOrDefaultBySpecificationAsync(
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

        return await context
            .Categories.ApplySpecification(specification: spec)
            .Include(c => c.ContentType)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        await context.Categories.AddAsync(category, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryPricingEntity>> GetPricingByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new CategoryPricingByCategorySpecification(categoryId: categoryId);
        return await context
            .CategoryPricing.ApplySpecification(specification: specification)
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
        var specification = new CategoryPricingByIdsSpecification(categoryId: categoryId, pricingTierId: pricingTierId);
        return await context
            .CategoryPricing.ApplySpecification(specification: specification)
            .Include(p => p.PricingTier)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddPricingAsync(CategoryPricingEntity pricing, CancellationToken cancellationToken = default)
    {
        await context.CategoryPricing.AddAsync(pricing, cancellationToken);
    }

    /// <inheritdoc />
    public void RemovePricing(CategoryPricingEntity pricing)
    {
        context.CategoryPricing.Remove(pricing);
    }
}
