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
public class PackageRepository(ContentDbContext context) : IPackageRepository
{
    /// <inheritdoc />
    public async Task<(List<PackageEntity> Packages, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<PackageEntity> query = context.Packages.Include(p => p.Slots).ThenInclude(s => s.Category);

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
    public async Task<PackageEntity?> GetByIdWithSlotsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new PackageByIdSpecification(id: id);
        return await context
            .Packages.ApplySpecification(specification: specification)
            .Include(p => p.Slots)
                .ThenInclude(s => s.Category)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PackageEntity> GetByIdWithSlotsOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PackageByIdSpecification(id: id);
        return await context
            .Packages.ApplySpecification(specification: specification)
            .Include(p => p.Slots)
                .ThenInclude(s => s.Category)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(PackageEntity package, CancellationToken cancellationToken = default)
    {
        await context.Packages.AddAsync(package, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PackageSlotEntity?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        var specification = new PackageSlotByIdSpecification(slotId: slotId);
        return await context.PackageSlots.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddSlotAsync(PackageSlotEntity slot, CancellationToken cancellationToken = default)
    {
        await context.PackageSlots.AddAsync(slot, cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveSlot(PackageSlotEntity slot)
    {
        context.PackageSlots.Remove(slot);
    }
}
