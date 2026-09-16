using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for package and package slot data access operations.
/// </summary>
public interface IPackageRepository
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
    /// Retrieves a package by its identifier, with its slots and their category pricing
    /// hydrated, or null when it does not exist.
    /// </summary>
    /// <param name="id">The unique identifier of the package.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The package if found, otherwise null.</returns>
    Task<PackageEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a package by its identifier, with its slots and their category pricing
    /// hydrated, tracked for mutation, or throws when it does not exist.
    /// </summary>
    /// <param name="id">The unique identifier of the package.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The package.</returns>
    Task<PackageEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new package to the repository.
    /// </summary>
    Task AddAsync(PackageEntity package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a modified package for the next commit. The write is explicit so it
    /// does not depend on the change tracker having observed the mutation.
    /// </summary>
    /// <param name="package">The modified package.</param>
    void Update(PackageEntity package);
}
