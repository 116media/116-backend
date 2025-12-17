using _116.Core.Domain.DTOs;
using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Domain;
using _116.Identity.Domain.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing role entities and role-related operations.
/// Provides methods for role retrieval, validation, and data aggregation.
/// </summary>
public interface IRoleRepository : IRepository<RoleEntity>
{
    /// <summary>
    /// Extracts and maps all roles assigned to a user.
    /// </summary>
    /// <param name="userRoles">Collection of user roles to process.</param>
    /// <returns>Collection of role DTOs mapped from user roles.</returns>
    /// <remarks>
    /// This method transforms UserRoleEntity collections into RoleDto collections
    /// for API response purposes.
    /// </remarks>
    IReadOnlyCollection<RoleDto> GetUserRoles(ICollection<UserRoleEntity> userRoles);

    /// <summary>
    /// Extracts and aggregates all unique permissions from user roles.
    /// </summary>
    /// <param name="userRoles">Collection of user roles to process.</param>
    /// <returns>Collection of unique permission DTOs aggregated from all user roles.</returns>
    /// <remarks>
    /// This method flattens permissions from multiple roles and removes duplicates.
    /// Use this when you need all permissions available to a user regardless of source role.
    /// </remarks>
    IReadOnlyCollection<PermissionDto> GetUserPermissions(ICollection<UserRoleEntity> userRoles);

    /// <summary>
    /// Extracts both roles and permissions from user roles in a single operation.
    /// </summary>
    /// <param name="userRoles">Collection of user roles to process.</param>
    /// <returns>Tuple containing mapped roles and aggregated unique permissions.</returns>
    /// <remarks>
    /// This method is optimized for scenarios where both roles and permissions are needed,
    /// performing the extraction in a single pass for better performance.
    /// </remarks>
    (IReadOnlyCollection<RoleDto> Roles, IReadOnlyCollection<PermissionDto> Permissions) GetUserRolesAndPermissions(
        ICollection<UserRoleEntity> userRoles
    );
}
