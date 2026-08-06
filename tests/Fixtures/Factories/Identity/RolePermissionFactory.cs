using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Identity;

namespace _116.Tests.Fixtures.Factories.Identity;

/// <summary>
/// Named aliases for <see cref="RolePermissionBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class RolePermissionFactory
{
    /// <summary>
    /// Creates a role-permission association with default random values.
    /// </summary>
    /// <returns>A new RolePermissionEntity with random values.</returns>
    public static RolePermissionEntity Create() => new RolePermissionBuilder().Build();

    /// <summary>
    /// Creates a role-permission association for a specific role and permission.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <returns>A new RolePermissionEntity with the specified role and permission.</returns>
    public static RolePermissionEntity Create(Guid roleId, Guid permissionId) =>
        new RolePermissionBuilder().ForRoleAndPermission(roleId, permissionId).Build();

    /// <summary>
    /// Creates a role-permission association with the Permission navigation property set.
    /// Useful for testing scenarios where the Permission relationship needs to be loaded.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="permission">The permission entity to associate.</param>
    /// <returns>A new RolePermissionEntity with the Permission navigation property set.</returns>
    public static RolePermissionEntity CreateWithPermission(Guid roleId, PermissionEntity permission) =>
        new RolePermissionBuilder().WithRoleId(roleId).WithPermission(permission).Build();

    /// <summary>
    /// Creates a role-permission association with a specific ID.
    /// </summary>
    /// <param name="id">The association identifier.</param>
    /// <returns>A new RolePermissionEntity with the specified ID.</returns>
    public static RolePermissionEntity CreateWithId(Guid id) => new RolePermissionBuilder().WithId(id).Build();
}
