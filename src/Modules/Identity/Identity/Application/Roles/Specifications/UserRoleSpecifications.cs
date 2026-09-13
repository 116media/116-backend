using System.Linq.Expressions;
using _116.Identity.Application.Shared.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Roles.Specifications;

/// <summary>
/// Specification that matches users with administrative privileges.
/// Includes both Admin and SuperAdmin roles, checking through the user's role associations.
/// </summary>
public class UserHasAdminRoleSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user =>
            user.UserRoles.Any(ur =>
                ur.Role.Name == nameof(EnumCoreUserRole.Admin) || ur.Role.Name == nameof(EnumCoreUserRole.SuperAdmin)
            );
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
        return activeSpec.And(other: adminSpec).ToExpression();
    }
}
