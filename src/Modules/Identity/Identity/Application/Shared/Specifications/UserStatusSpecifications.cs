using System.Linq.Expressions;

using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Shared.Specifications;

/// <summary>
/// Specification that matches only active user accounts.
/// Active users are allowed to authenticate and access the system.
/// </summary>
public class UserIsActiveSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.IsActive;
    }
}
/// <summary>
/// Specification that matches verified user accounts.
/// For local authentication users, verification is required.
/// Social authentication users are automatically considered verified.
/// Returns true for non-local auth providers or when IsVerified is true for local auth.
/// </summary>
public class UserIsVerifiedSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.AuthProvider != EnumAuthProvider.Local || user.IsVerified;
    }
}
/// <summary>
/// Composite specification for active and verified public users.
/// Combines IsActive and IsVerified specifications, commonly used for public user authentication flows.
/// </summary>
public class UserIsActiveAndVerifiedSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        var activeSpec = new UserIsActiveSpecification();
        var verifiedSpec = new UserIsVerifiedSpecification();
        return activeSpec.And(verifiedSpec).ToExpression();
    }
}
