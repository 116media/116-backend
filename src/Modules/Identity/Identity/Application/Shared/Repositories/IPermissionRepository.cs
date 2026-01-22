using _116.Identity.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing permission entities and permission-related operations.
/// Provides methods for permission retrieval, validation, and data access.
/// </summary>
public interface IPermissionRepository : IRepository<PermissionEntity>
{
    /// <summary>
    /// Retrieves a permission by its unique identifier.
    /// </summary>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The permission entity if found.</returns>
    /// <exception cref="Shared.Application.Exceptions.NotFoundException">
    /// Thrown when no permission is found with the specified ID.
    /// </exception>
    Task<PermissionEntity?> GetPermissionByIdOrThrowAsync(
        Guid permissionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if a permission with the specified resource and action already exists.
    /// </summary>
    /// <param name="resource">The resource name to check.</param>
    /// <param name="action">The action name to check.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if a permission with the resource and action exists, otherwise false.</returns>
    Task<bool> ExistsByResourceAndActionAsync(
        string resource,
        string action,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new permission entity to the repository.
    /// </summary>
    /// <param name="permission">The permission entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(PermissionEntity permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions with pagination and optional filtering.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional search term for fuzzy matching on Resource, Action, and Description.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <param name="isDeleted">Optional filter by deleted status.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>Tuple of permissions list and total count.</returns>
    Task<(List<PermissionEntity> Permissions, int TotalCount)> GetAllWithPaginationAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        bool? isDeleted = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Permanently deletes a permission entity from the repository.
    /// </summary>
    /// <param name="entity">The permission entity to delete.</param>
    void Delete(PermissionEntity entity);
}
