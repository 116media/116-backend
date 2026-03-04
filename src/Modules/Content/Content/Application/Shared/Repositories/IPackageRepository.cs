using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for package and package slot data access operations.
/// </summary>
public interface IPackageRepository : IRepository<PackageEntity>
{
    /// <summary>
    /// Retrieves a paginated list of packages with an optional active status filter.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of packages and the total count.</returns>
    Task<(List<PackageEntity> Packages, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a package by its unique identifier including its slots and their categories.
    /// Returns null if not found.
    /// </summary>
    Task<PackageEntity?> GetByIdWithSlotsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a package by its unique identifier including its slots and their categories.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the package is not found.</exception>
    Task<PackageEntity> GetByIdWithSlotsOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Adds a new package to the repository.</summary>
    Task AddAsync(PackageEntity package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a package slot by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    Task<PackageSlotEntity?> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default);

    /// <summary>Adds a new package slot to the repository.</summary>
    Task AddSlotAsync(PackageSlotEntity slot, CancellationToken cancellationToken = default);

    /// <summary>Removes a package slot from the repository.</summary>
    void RemoveSlot(PackageSlotEntity slot);
}
