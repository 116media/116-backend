namespace _116.Identity.Application.Shared.Cache;

/// <summary>
/// In-process cache of the per-user <see cref="UserSecurityState" /> markers checked on every
/// authenticated request. Read-through on a miss, evicted on a bump.
/// </summary>
public interface IUserSecurityStateCache
{
    /// <summary>
    /// Current stamp/version for the user, loaded from the database on a cache miss. A user
    /// without a token-state row yields the default state, which matches no real token.
    /// </summary>
    /// <param name="userId">The user the markers belong to.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current security state for the user.</returns>
    Task<UserSecurityState> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Caches a state the caller already holds, sparing the next read a database round trip.
    /// </summary>
    /// <param name="userId">The user the markers belong to.</param>
    /// <param name="state">The state that is now current.</param>
    void Set(Guid userId, UserSecurityState state);

    /// <summary>
    /// Drops the cached state so the next read reloads from the database.
    /// </summary>
    /// <param name="userId">The user whose cached state is dropped.</param>
    void Remove(Guid userId);
}

/// <summary>
/// The current pair of per-user token-invalidation markers, compared against the
/// <c>sstamp</c>/<c>tver</c> claims of every authenticated request.
/// </summary>
/// <param name="SecurityStamp">The current security stamp.</param>
/// <param name="TokenVersion">The current token version.</param>
public readonly record struct UserSecurityState(Guid SecurityStamp, long TokenVersion);
