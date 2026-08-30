using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IUserTokenStateRepository" /> using Entity Framework Core, with
/// set-based <c>ExecuteUpdateAsync</c> bumps that evict the cached state.
/// </summary>
/// <param name="context">The Identity database context.</param>
/// <param name="securityStateCache">Cache kept consistent with every marker bump.</param>
public class UserTokenStateRepository(IdentityDbContext context, IUserSecurityStateCache securityStateCache)
    : IUserTokenStateRepository
{
    /// <inheritdoc />
    public async Task AddAsync(UserTokenStateEntity state, CancellationToken cancellationToken)
    {
        await context.UserTokenStates.AddAsync(entity: state, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserSecurityState?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        UserSecurityState[] projection = await context
            .UserTokenStates.Where(s => s.Id == userId)
            .Select(s => new UserSecurityState(s.SecurityStamp, s.TokenVersion))
            .ToArrayAsync(cancellationToken: cancellationToken);

        return projection.Length > 0 ? projection[0] : null;
    }

    /// <inheritdoc />
    public async Task<UserSecurityState> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        UserSecurityState? existing = await GetAsync(userId: userId, cancellationToken: cancellationToken);
        if (existing is not null)
        {
            return existing.Value;
        }

        var record = UserTokenStateEntity.Create(userId: userId);
        await context.UserTokenStates.AddAsync(entity: record, cancellationToken: cancellationToken);
        await context.SaveChangesAsync(cancellationToken: cancellationToken);

        var state = new UserSecurityState(SecurityStamp: record.SecurityStamp, TokenVersion: record.TokenVersion);
        securityStateCache.Set(userId: userId, state: state);
        return state;
    }

    /// <inheritdoc />
    public async Task BumpTokenVersionAsync(Guid userId, CancellationToken cancellationToken)
    {
        await context
            .UserTokenStates.Where(s => s.Id == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.TokenVersion, x => x.TokenVersion + 1),
                cancellationToken: cancellationToken
            );

        securityStateCache.Remove(userId: userId);
    }

    /// <inheritdoc />
    public async Task BumpTokenVersionForRoleUsersAsync(Guid roleId, CancellationToken cancellationToken)
    {
        List<Guid> affectedUserIds = await context
            .UserRoles.Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken: cancellationToken);

        if (affectedUserIds.Count == 0)
        {
            return;
        }

        await context
            .UserTokenStates.Where(s => affectedUserIds.Contains(s.Id))
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.TokenVersion, x => x.TokenVersion + 1),
                cancellationToken: cancellationToken
            );

        foreach (Guid userId in affectedUserIds)
        {
            securityStateCache.Remove(userId: userId);
        }
    }

    /// <inheritdoc />
    public async Task<Guid> RotateSecurityStampAsync(Guid userId, CancellationToken cancellationToken)
    {
        var newStamp = Guid.NewGuid();

        await context
            .UserTokenStates.Where(s => s.Id == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.SecurityStamp, newStamp),
                cancellationToken: cancellationToken
            );

        securityStateCache.Remove(userId: userId);
        return newStamp;
    }
}
