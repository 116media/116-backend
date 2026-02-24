using _116.Identity.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing role entities and role-related operations.
/// Provides methods for role retrieval, validation, and data aggregation.
/// </summary>
public interface IRoleRepository : IRepository<RoleEntity>
{
    /// <summary>
    /// Retrieves a role by its unique identifier with its associated permissions.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The role entity with permissions loaded.</returns>
    /// <exception cref="Shared.Application.Exceptions.NotFoundException">
    /// Thrown when no role is found with the specified ID.
    /// </exception>
    Task<RoleEntity?> GetRoleByIdWithPermissionsOrThrowAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a role by its unique identifier.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The role entity if found.</returns>
    /// <exception cref="Shared.Application.Exceptions.NotFoundException">
    /// Thrown when no role is found with the specified ID.
    /// </exception>
    Task<RoleEntity?> GetRoleByIdOrThrowAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role with the specified name already exists.
    /// </summary>
    /// <param name="name">The role name to check.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if a role with the name exists, otherwise false.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new role entity to the repository.
    /// </summary>
    /// <param name="role">The role entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(RoleEntity role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles with pagination and optional filtering.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional search term for fuzzy matching on Name and Description.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <param name="isDeleted">Optional filter by deleted status.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>Tuple of roles list and total count.</returns>
    Task<(List<RoleEntity> Roles, int TotalCount)> GetAllWithPaginationAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        bool? isDeleted = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Permanently deletes a role entity from the repository.
    /// </summary>
    /// <param name="entity">The role entity to delete.</param>
    void Delete(RoleEntity entity);
}
