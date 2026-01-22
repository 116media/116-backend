using _116.Identity.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing user-role associations.
/// Provides methods for user-role retrieval, validation, and management.
/// </summary>
public interface IUserRoleRepository : IRepository<UserRoleEntity>
{
    /// <summary>
    /// Checks if a user-role association already exists.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleId">The role ID.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if the association exists, otherwise false.</returns>
    Task<bool> ExistsByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user-role association by user ID and role ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleId">The role ID.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The user-role entity if found, otherwise null.</returns>
    Task<UserRoleEntity?> GetByUserAndRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets all user-role associations for a user with roles loaded.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>List of user-role entities with roles loaded.</returns>
    Task<List<UserRoleEntity>> GetUserRolesWithRoleAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user-role association to the repository.
    /// </summary>
    /// <param name="entity">The user-role entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(UserRoleEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user-role association from the repository.
    /// </summary>
    /// <param name="entity">The user-role entity to remove.</param>
    void Delete(UserRoleEntity entity);
}
