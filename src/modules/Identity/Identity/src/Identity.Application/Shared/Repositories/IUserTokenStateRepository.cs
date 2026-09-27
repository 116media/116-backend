using _116.Identity.Application.Shared.Cache;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository for the per-user token-invalidation record. Bumps are atomic SQL updates that evict
/// the affected users from <see cref="IUserSecurityStateCache" />.
/// </summary>
public interface IUserTokenStateRepository
{
    /// <summary>
    /// Adds the record for a newly created user (same unit of work as the user).
    /// </summary>
    /// <param name="state">The invalidation record to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(UserTokenStateEntity state, CancellationToken cancellationToken);

    /// <summary>
    /// The current stamp/version projection, or null if the row is missing.
    /// </summary>
    /// <param name="userId">The user the record belongs to.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current security state, or null when no row exists.</returns>
    Task<UserSecurityState?> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// The current stamp/version projection, creating and persisting the record when it does not
    /// exist yet.
    /// </summary>
    /// <param name="userId">The user the record belongs to.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current security state for the user.</returns>
    Task<UserSecurityState> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically increments the token version (no read-modify-write).
    /// </summary>
    /// <param name="userId">The user whose token version is bumped.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task BumpTokenVersionAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically increments the token version of every user holding the role, in one set-based
    /// update.
    /// </summary>
    /// <param name="roleId">The role whose members' token versions are bumped.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task BumpTokenVersionForRoleUsersAsync(Guid roleId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically rotates the security stamp to a fresh value and returns it.
    /// </summary>
    /// <param name="userId">The user whose security stamp is rotated.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The freshly generated security stamp.</returns>
    Task<Guid> RotateSecurityStampAsync(Guid userId, CancellationToken cancellationToken);
}
