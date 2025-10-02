using _116.Shared.Application.Specifications;
using _116.Auth.Domain.Entities;
using System.Linq.Expressions;

namespace _116.Auth.Application.Shared.Specifications;

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
