using _116.Shared.Application.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using System.Linq.Expressions;

namespace _116.Identity.Application.Shared.Specifications;

/// <summary>
/// Specification that matches users with administrative privileges.
/// Includes both Admin and SuperAdmin roles, checking through the user's role associations.
/// </summary>
public class UserHasAdminRoleSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.UserRoles.Any(ur =>
            ur.Role.Name == nameof(EnumCoreUserRole.Admin) ||
            ur.Role.Name == nameof(EnumCoreUserRole.SuperAdmin));
    }
}

/// <summary>
/// Specification that matches users with a specific role.
/// </summary>
public class UserHasRoleSpecification(string roleName) : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.UserRoles.Any(ur => ur.Role.Name == roleName);
    }
}

/// <summary>
/// Specification that matches users with the Visitor role.
/// </summary>
public class UserHasVisitorRoleSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.UserRoles.Any(ur => ur.Role.Name == nameof(EnumCoreUserRole.Visitor));
    }
}

/// <summary>
/// Composite specification for active admin users.
/// Combines IsActive and HasAdminRole specifications, commonly used for admin authentication flows.
/// </summary>
public class UserIsActiveAdminSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        var activeSpec = new UserIsActiveSpecification();
        var adminSpec = new UserHasAdminRoleSpecification();

        return activeSpec.And(adminSpec).ToExpression();
    }
}
