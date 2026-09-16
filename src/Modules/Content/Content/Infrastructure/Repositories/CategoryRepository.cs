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
        IQueryable<CategoryEntity> query = Context.Categories.Include(c => c.Pricing);

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
    protected override IQueryable<CategoryEntity> Query()
    {
        return Context.Categories.Include(c => c.Pricing);
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
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetGossipCategoryAsync(CancellationToken cancellationToken = default)
    {
        var specification = new GossipCategorySpecification();
        return await Context
            .Categories.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetExclusiveCategoryAsync(CancellationToken cancellationToken = default)
    {
        var specification = new ExclusiveCategorySpecification();
        return await Context
            .Categories.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryEntity?> GetDefaultLyricsCategoryAsync(CancellationToken cancellationToken = default)
    {
        var specification = new DefaultLyricsCategorySpecification();
        return await Context
            .Categories.AsTracking()
            .ApplySpecification(specification: specification)
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
            .OrderByDescending(c => c.PinnedToFeedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, CategoryEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    )
    {
        List<CategoryEntity> entities = await Query()
            .Where(entity => ids.Contains(entity.Id))
            .ToListAsync(cancellationToken);

        return entities.ToDictionary(entity => entity.Id);
    }
}
