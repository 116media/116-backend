using System.Linq.Expressions;

using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Roles.Specifications;

/// <summary>
/// Specification that matches role-permission associations by role ID.
/// Used for finding all permissions assigned to a specific role.
/// </summary>
public class RolePermissionByRoleIdSpecification(Guid roleId) : Specification<RolePermissionEntity>
{
    public override Expression<Func<RolePermissionEntity, bool>> ToExpression()
    {
        return rolePermission => rolePermission.RoleId == roleId;
    }
}

/// <summary>
/// Specification that matches role-permission associations by permission ID.
/// Used for finding all roles that have a specific permission.
/// </summary>
public class RolePermissionByPermissionIdSpecification(Guid permissionId) : Specification<RolePermissionEntity>
{
    public override Expression<Func<RolePermissionEntity, bool>> ToExpression()
    {
        return rolePermission => rolePermission.PermissionId == permissionId;
    }
}

/// <summary>
/// Specification that matches role-permission associations by unique identifier.
/// Used for direct role-permission association lookup operations.
/// </summary>
public class RolePermissionByIdSpecification(Guid rolePermissionId) : Specification<RolePermissionEntity>
{
    public override Expression<Func<RolePermissionEntity, bool>> ToExpression()
    {
        return rolePermission => rolePermission.Id == rolePermissionId;
    }
}

/// <summary>
/// Composite specification that matches a specific role-permission association.
/// Combines role ID and permission ID specifications.
/// Used for checking if a specific permission is assigned to a specific role.
/// </summary>
public class RolePermissionByRoleAndPermissionSpecification(Guid roleId, Guid permissionId)
    : Specification<RolePermissionEntity>
{
    public override Expression<Func<RolePermissionEntity, bool>> ToExpression()
    {
        var roleSpec = new RolePermissionByRoleIdSpecification(roleId: roleId);
        var permissionSpec = new RolePermissionByPermissionIdSpecification(permissionId: permissionId);
        return roleSpec.And(other: permissionSpec).ToExpression();
    }
}
