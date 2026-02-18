using System.Reflection;
using _116.Identity.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities;

/// <summary>
/// Fluent builder for creating <see cref="RolePermissionEntity"/> instances in tests.
/// For complex junction table scenarios.
/// </summary>
internal class RolePermissionBuilder
{
    private Guid _id;
    private Guid _roleId;
    private Guid _permissionId;
    private PermissionEntity? _permission;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolePermissionBuilder"/> class with random default values.
    /// </summary>
    public RolePermissionBuilder()
    {
        _id = Guid.NewGuid();
        _roleId = Guid.NewGuid();
        _permissionId = Guid.NewGuid();
    }

    /// <summary>
    /// Sets the role-permission association ID.
    /// </summary>
    /// <param name="id">The association identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RolePermissionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the role ID.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RolePermissionBuilder WithRoleId(Guid roleId)
    {
        _roleId = roleId;
        return this;
    }

    /// <summary>
    /// Sets the permission ID.
    /// </summary>
    /// <param name="permissionId">The permission identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RolePermissionBuilder WithPermissionId(Guid permissionId)
    {
        _permissionId = permissionId;
        return this;
    }

    /// <summary>
    /// Sets both role and permission IDs.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RolePermissionBuilder ForRoleAndPermission(Guid roleId, Guid permissionId)
    {
        _roleId = roleId;
        _permissionId = permissionId;
        return this;
    }

    /// <summary>
    /// Sets the permission entity for navigation property testing.
    /// </summary>
    /// <param name="permission">The permission entity.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RolePermissionBuilder WithPermission(PermissionEntity permission)
    {
        _permission = permission;
        _permissionId = permission.Id;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="RolePermissionEntity"/> instance.
    /// </summary>
    /// <returns>A configured RolePermissionEntity instance.</returns>
    public RolePermissionEntity Build()
    {
        var rolePermission = RolePermissionEntity.Create(_id, _roleId, _permissionId);

        if (_permission is not null)
        {
            SetNavigationProperty(rolePermission, "Permission", _permission);
        }

        return rolePermission;
    }

    /// <summary>
    /// Sets a navigation property using reflection for testing purposes.
    /// </summary>
    private static void SetNavigationProperty<T>(RolePermissionEntity entity, string propertyName, T value)
    {
        PropertyInfo? property = typeof(RolePermissionEntity).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance
        );
        property?.SetValue(entity, value);
    }
}
