using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing user login sessions.
/// Handles creating, retrieving, and revoking user sessions for refresh token management.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Creates a new session for a user login.
    /// </summary>
    /// <param name="session">The session entity to create.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task CreateAsync(SessionEntity session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a session by its refresh token hash.
    /// </summary>
    /// <param name="refreshTokenHash">The hashed refresh token.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The session entity if found, null otherwise.</returns>
    Task<SessionEntity?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a specific session (for logout).
    /// </summary>
    /// <param name="sessionId">The ID of the session to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all sessions for a user (for logout from all devices).
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
