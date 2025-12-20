using System.Linq.Expressions;

using _116.Identity.Domain.Entities;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Shared.Specifications;

/// <summary>
/// Specification that matches sessions by user ID.
/// Used for filtering sessions belonging to a specific user.
/// </summary>
public class SessionByUserIdSpecification(Guid userId) : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        return session => session.UserId == userId;
    }
}

/// <summary>
/// Specification that matches sessions by refresh token hash.
/// Used for finding a specific session during token refresh operations.
/// </summary>
public class SessionByRefreshTokenHashSpecification(string refreshTokenHash) : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        return session => session.RefreshTokenHash == refreshTokenHash;
    }
}

/// <summary>
/// Specification that matches sessions by unique identifier.
/// Used for direct session lookup operations.
/// </summary>
public class SessionByIdSpecification(Guid sessionId) : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        return session => session.Id == sessionId;
    }
}

/// <summary>
/// Specification that matches non-deleted sessions.
/// Used for filtering sessions that are still active and not marked as deleted.
/// </summary>
public class SessionIsNotDeletedSpecification : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        return session => session.IsDeleted == false;
    }
}

/// <summary>
/// Specification that matches deleted sessions.
/// Used for filtering sessions that have been revoked or logged out.
/// </summary>
public class SessionIsDeletedSpecification : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        return session => session.IsDeleted == true;
    }
}

/// <summary>
/// Specification that matches non-expired sessions.
/// Used for filtering sessions that are still valid based on expiration time.
/// </summary>
public class SessionIsNotExpiredSpecification : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        return session => session.ExpiresAt > DateTime.UtcNow;
    }
}

/// <summary>
/// Specification that matches expired sessions.
/// Used for cleanup operations and filtering expired sessions.
/// </summary>
public class SessionIsExpiredSpecification : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        return session => session.ExpiresAt <= DateTime.UtcNow;
    }
}

/// <summary>
/// Composite specification that matches active sessions.
/// Combines not deleted and not expired specifications.
/// Used for finding sessions that are currently valid.
/// </summary>
public class SessionIsActiveSpecification : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        var notDeletedSpec = new SessionIsNotDeletedSpecification();
        var notExpiredSpec = new SessionIsNotExpiredSpecification();
        return notDeletedSpec.And(notExpiredSpec).ToExpression();
    }
}

/// <summary>
/// Composite specification that matches active sessions for a specific user.
/// Combines user ID, not deleted, and not expired specifications.
/// Used for finding all valid sessions belonging to a user.
/// </summary>
public class ActiveSessionsByUserIdSpecification(Guid userId) : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        var userSpec = new SessionByUserIdSpecification(userId);
        var activeSpec = new SessionIsActiveSpecification();
        return userSpec.And(activeSpec).ToExpression();
    }
}

/// <summary>
/// Composite specification that matches valid refresh token sessions.
/// Combines refresh token hash and active specifications.
/// Used for validating refresh tokens during token refresh operations.
/// </summary>
public class ValidRefreshTokenSessionSpecification(string refreshTokenHash) : Specification<SessionEntity>
{
    public override Expression<Func<SessionEntity, bool>> ToExpression()
    {
        var tokenSpec = new SessionByRefreshTokenHashSpecification(refreshTokenHash);
        var activeSpec = new SessionIsActiveSpecification();
        return tokenSpec.And(activeSpec).ToExpression();
    }
}
