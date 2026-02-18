using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities;

namespace _116.Tests.Fixtures.Factories;

/// <summary>
/// Factory for quickly creating <see cref="RolePermissionEntity"/> instances in tests.
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

    /// <summary>
    /// Creates a list of role-permission associations with the specified count.
    /// </summary>
    /// <param name="count">The number of associations to create.</param>
    /// <returns>A list of RolePermissionEntity instances.</returns>
    public static List<RolePermissionEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
