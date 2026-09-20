using _116.Content.Application.Catalog.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IPackageRepository" /> for managing package and package slot entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class PackageRepository(ContentDbContext context) : ContentRepository<PackageEntity>(context), IPackageRepository
{
    /// <inheritdoc />
    public async Task<(List<PackageEntity> Packages, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<PackageEntity> query = Context.Packages.Include(p => p.Slots);

        if (isActive.HasValue)
        {
            Specification<PackageEntity> spec = isActive.Value
                ? new ActivePackageSpecification()
                : new InactivePackageSpecification();
            query = query.ApplySpecification(specification: spec);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<PackageEntity> packages = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (packages, totalCount);
    }

    /// <inheritdoc />
    protected override IQueryable<PackageEntity> Query()
    {
        return Context.Packages.Include(p => p.Slots);
    }
}
