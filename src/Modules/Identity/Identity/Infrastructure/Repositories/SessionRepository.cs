using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;

using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing user login sessions with soft delete support.
/// </summary>
public class SessionRepository(IdentityDbContext context) : ISessionRepository
{
    /// <inheritdoc />
    public async Task CreateAsync(SessionEntity session, CancellationToken cancellationToken = default)
    {
        await context.Sessions.AddAsync(entity: session, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SessionEntity?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default
    )
    {
        var spec = new ValidRefreshTokenSessionSpecification(refreshTokenHash: refreshTokenHash);
        return await context.Sessions
            .Where(spec.ToExpression())
            .Include(s => s.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var idSpec = new SessionByIdSpecification(sessionId: sessionId);
        var notDeletedSpec = new SessionIsNotDeletedSpecification();
        Specification<SessionEntity> spec = idSpec.And(other: notDeletedSpec);

        SessionEntity? session = await context.Sessions
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        session?.Delete();
    }

    /// <inheritdoc />
    public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var spec = new ActiveSessionsByUserIdSpecification(userId: userId);
        List<SessionEntity> sessions = await context.Sessions
            .Where(spec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (SessionEntity session in sessions)
        {
            session.Delete();
        }
    }

    /// <inheritdoc />
    public async Task DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSpec = new SessionIsExpiredSpecification();
        var notDeletedSpec = new SessionIsNotDeletedSpecification();
        Specification<SessionEntity> spec = expiredSpec.And(other: notDeletedSpec);

        List<SessionEntity> expiredSessions = await context.Sessions
            .Where(spec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (SessionEntity session in expiredSessions)
        {
            session.Delete();
        }
    }
}
