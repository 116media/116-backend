using _116.Identity.Application.Session.Builders;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
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
        return await context
            .Sessions.Where(spec.ToExpression())
            .Include(s => s.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var idSpec = new SessionByIdSpecification(sessionId: sessionId);
        var notRevokedSpec = new SessionIsNotRevokedSpecification();
        Specification<SessionEntity> spec = idSpec.And(other: notRevokedSpec);

        SessionEntity? session = await context
            .Sessions.Where(spec.ToExpression())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        session?.Revoke();
    }

    /// <inheritdoc />
    public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var spec = new ActiveSessionsByUserIdSpecification(userId: userId);
        List<SessionEntity> sessions = await context
            .Sessions.Where(spec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (SessionEntity session in sessions)
        {
            session.Revoke();
        }
    }

    /// <inheritdoc />
    public async Task<int> DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSpec = new SessionIsExpiredSpecification();
        var notRevokedSpec = new SessionIsNotRevokedSpecification();
        Specification<SessionEntity> spec = expiredSpec.And(other: notRevokedSpec);

        List<SessionEntity> expiredSessions = await context
            .Sessions.Where(spec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (SessionEntity session in expiredSessions)
        {
            session.Revoke();
        }

        return expiredSessions.Count;
    }

    /// <inheritdoc />
    public async Task<SessionEntity?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var idSpec = new SessionByIdSpecification(sessionId: sessionId);
        var notRevokedSpec = new SessionIsNotRevokedSpecification();
        Specification<SessionEntity> spec = idSpec.And(other: notRevokedSpec);

        return await context
            .Sessions.Where(spec.ToExpression())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<SessionEntity>> GetUserSessionsAsync(
        Guid userId,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    )
    {
        Specification<SessionEntity> spec = new SessionQueryBuilder()
            .WithUserId(userId: userId)
            .WithActiveStatus(isActive: isActive)
            .Build()!;

        return await context
            .Sessions.Where(spec.ToExpression())
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<SessionEntity> sessions, int totalCount)> GetAllWithPaginationAsync(
        int page,
        int pageSize,
        string? status = null,
        Guid? userId = null,
        string? ipAddress = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default
    )
    {
        ISessionQueryBuilder builder = new SessionQueryBuilder()
            .WithStatus(status: status)
            .WithIpAddress(ipAddress: ipAddress)
            .WithFromDate(fromDate: fromDate)
            .WithToDate(toDate: toDate);

        if (userId.HasValue)
        {
            builder = builder.WithUserId(userId: userId.Value);
        }

        Specification<SessionEntity>? spec = builder.Build();

        IQueryable<SessionEntity> query = spec is not null
            ? context.Sessions.Where(spec.ToExpression())
            : context.Sessions;

        int totalCount = await query.CountAsync(cancellationToken: cancellationToken);

        List<SessionEntity> sessions = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return (sessions, totalCount);
    }

    /// <inheritdoc />
    public async Task<Dictionary<EnumBrowser, int>> GetActiveSessionCountByBrowserAsync(
        CancellationToken cancellationToken = default
    )
    {
        var activeSpec = new SessionIsActiveSpecification();
        List<SessionEntity> activeSessions = await context
            .Sessions.Where(activeSpec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        return activeSessions.GroupBy(s => s.Browser).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <inheritdoc />
    public async Task<Dictionary<EnumDevice, int>> GetActiveSessionCountByDeviceAsync(
        CancellationToken cancellationToken = default
    )
    {
        var activeSpec = new SessionIsActiveSpecification();
        List<SessionEntity> activeSessions = await context
            .Sessions.Where(activeSpec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        return activeSessions.GroupBy(s => s.Device).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <inheritdoc />
    public async Task<Dictionary<EnumPlatform, int>> GetActiveSessionCountByPlatformAsync(
        CancellationToken cancellationToken = default
    )
    {
        var activeSpec = new SessionIsActiveSpecification();
        List<SessionEntity> activeSessions = await context
            .Sessions.Where(activeSpec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        return activeSessions.GroupBy(s => s.Platform).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <inheritdoc />
    public async Task<Dictionary<EnumClient, int>> GetActiveSessionCountByClientAsync(
        CancellationToken cancellationToken = default
    )
    {
        var activeSpec = new SessionIsActiveSpecification();
        List<SessionEntity> activeSessions = await context
            .Sessions.Where(activeSpec.ToExpression())
            .ToListAsync(cancellationToken: cancellationToken);

        return activeSessions.GroupBy(s => s.Client).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <inheritdoc />
    public async Task<int> GetTotalActiveSessionsCountAsync(CancellationToken cancellationToken = default)
    {
        var activeSpec = new SessionIsActiveSpecification();
        return await context.Sessions.Where(activeSpec.ToExpression()).CountAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetTotalActiveUsersCountAsync(CancellationToken cancellationToken = default)
    {
        var activeSpec = new SessionIsActiveSpecification();
        return await context
            .Sessions.Where(activeSpec.ToExpression())
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<SessionEntity>> GetSessionsForExportAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default
    )
    {
        Specification<SessionEntity>? spec = new SessionQueryBuilder()
            .WithStatus(status: status)
            .WithFromDate(fromDate: fromDate)
            .WithToDate(toDate: toDate)
            .Build();

        IQueryable<SessionEntity> query = spec is not null
            ? context.Sessions.Where(spec.ToExpression())
            : context.Sessions;

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateRefreshTokenAsync(
        Guid sessionId,
        string newRefreshTokenHash,
        DateTime newExpiresAt,
        CancellationToken cancellationToken = default
    )
    {
        var idSpec = new SessionByIdSpecification(sessionId: sessionId);
        var activeSpec = new SessionIsActiveSpecification();
        Specification<SessionEntity> spec = idSpec.And(other: activeSpec);

        SessionEntity? session = await context
            .Sessions.Where(spec.ToExpression())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        session?.UpdateRefreshToken(newRefreshTokenHash: newRefreshTokenHash, newExpiresAt: newExpiresAt);
    }
}
