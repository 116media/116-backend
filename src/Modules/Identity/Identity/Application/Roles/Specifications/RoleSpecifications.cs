using System.Linq.Expressions;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Roles.Specifications;

/// <summary>
/// Specification that matches roles by their name.
/// Used for role lookup operations by name.
/// </summary>
public class RoleByNameSpecification(string roleName) : Specification<RoleEntity>
{
    public override Expression<Func<RoleEntity, bool>> ToExpression()
    {
        return role => role.Name == roleName;
    }
}

/// <summary>
/// Specification that matches roles by their unique identifier.
/// Used for direct role lookup operations.
/// </summary>
public class RoleByIdSpecification(Guid roleId) : Specification<RoleEntity>
{
    public override Expression<Func<RoleEntity, bool>> ToExpression()
    {
        return role => role.Id == roleId;
    }
}

/// <summary>
/// Specification that matches active roles.
/// </summary>
public class RoleIsActiveSpecification : Specification<RoleEntity>
{
    public override Expression<Func<RoleEntity, bool>> ToExpression()
    {
        return role => role.IsActive;
    }
}

/// <summary>
/// Specification that matches soft-deleted roles.
/// </summary>
public class RoleIsDeletedSpecification : Specification<RoleEntity>
{
    public override Expression<Func<RoleEntity, bool>> ToExpression()
    {
        return role => role.IsDeleted;
    }
}

/// <summary>
/// Specification that matches non-deleted roles.
/// </summary>
public class RoleNotDeletedSpecification : Specification<RoleEntity>
{
    public override Expression<Func<RoleEntity, bool>> ToExpression()
    {
        return role => !role.IsDeleted;
    }
}

/// <summary>
/// Specification that matches active and non-deleted roles.
/// Used for listing available roles.
/// </summary>
public class ActiveRoleSpecification : Specification<RoleEntity>
{
    public override Expression<Func<RoleEntity, bool>> ToExpression()
    {
        return role => role.IsActive && !role.IsDeleted;
    }
}
