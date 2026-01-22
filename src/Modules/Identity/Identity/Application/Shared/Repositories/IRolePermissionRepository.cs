using _116.Identity.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing role-permission associations.
/// Provides methods for role-permission retrieval, validation, and management.
/// </summary>
public interface IRolePermissionRepository : IRepository<RolePermissionEntity>
{
    /// <summary>
    /// Checks if a role-permission association already exists.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if the association exists, otherwise false.</returns>
    Task<bool> ExistsByRoleAndPermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a role-permission association by role ID and permission ID.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The role-permission entity if found, otherwise null.</returns>
    Task<RolePermissionEntity?> GetByRoleAndPermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new role-permission association to the repository.
    /// </summary>
    /// <param name="entity">The role-permission entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(RolePermissionEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role-permission association from the repository.
    /// </summary>
    /// <param name="entity">The role-permission entity to remove.</param>
    void Delete(RolePermissionEntity entity);

    /// <summary>
    /// Gets all permission IDs assigned to a role.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>List of permission IDs assigned to the role.</returns>
    Task<List<Guid>> GetPermissionIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
