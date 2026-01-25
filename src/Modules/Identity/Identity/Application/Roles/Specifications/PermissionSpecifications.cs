using System.Linq.Expressions;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Application.Roles.Specifications;

/// <summary>
/// Specification that matches permissions by their unique identifier.
/// </summary>
public class PermissionByIdSpecification(Guid permissionId) : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        return permission => permission.Id == permissionId;
    }
}

/// <summary>
/// Specification that matches permissions by resource and action.
/// Used for checking if a permission already exists.
/// </summary>
public class PermissionByResourceAndActionSpecification(string resource, string action)
    : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        return permission => permission.Resource == resource && permission.Action == action;
    }
}

/// <summary>
/// Specification that matches active permissions.
/// </summary>
public class PermissionIsActiveSpecification : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        return permission => permission.IsActive;
    }
}

/// <summary>
/// Specification that matches soft-deleted permissions.
/// </summary>
public class PermissionIsDeletedSpecification : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        return permission => permission.IsDeleted;
    }
}

/// <summary>
/// Specification that matches non-deleted permissions.
/// </summary>
public class PermissionNotDeletedSpecification : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        return permission => !permission.IsDeleted;
    }
}

/// <summary>
/// Specification that matches active and non-deleted permissions.
/// Used for listing available permissions.
/// </summary>
public class ActivePermissionSpecification : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        return permission => permission.IsActive && !permission.IsDeleted;
    }
}

/// <summary>
/// Specification that matches inactive permissions.
/// </summary>
public class PermissionIsNotActiveSpecification : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        return permission => !permission.IsActive;
    }
}

/// <summary>
/// Specification for fuzzy search across permission Resource, Action, and Description.
/// Uses case-insensitive matching (ILIKE in PostgreSQL).
/// </summary>
public class PermissionSearchSpecification(string search) : Specification<PermissionEntity>
{
    public override Expression<Func<PermissionEntity, bool>> ToExpression()
    {
        string pattern = $"%{search}%";
        return permission =>
            EF.Functions.ILike(permission.Resource, pattern)
            || EF.Functions.ILike(permission.Action, pattern)
            || EF.Functions.ILike(permission.Description, pattern);
    }
}
