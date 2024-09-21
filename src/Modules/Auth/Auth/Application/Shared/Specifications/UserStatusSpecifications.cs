using _116.Shared.Application.Specifications;
using _116.Auth.Domain.Entities;
using System.Linq.Expressions;
using AuthProvider = _116.Auth.Domain.Enums.AuthProvider;

namespace _116.Auth.Application.Shared.Specifications;

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
        return user => user.AuthProvider != AuthProvider.Local || user.IsVerified;
    }
}

/// <summary>
/// Specification that matches currently logged-in users.
/// Used to validate active user sessions and prevent operations on logged-out users.
/// </summary>
public class UserIsLoggedInSpecification : Specification<UserEntity>
{
    public override Expression<Func<UserEntity, bool>> ToExpression()
    {
        return user => user.IsLoggedIn;
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
