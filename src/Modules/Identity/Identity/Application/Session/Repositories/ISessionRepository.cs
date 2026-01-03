using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Session.Repositories;

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

    /// <summary>
    /// Gets a session by its ID.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The session entity if found, null otherwise.</returns>
    Task<SessionEntity?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sessions for a user with optional filtering by active status.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="isActive">Optional filter: true for active only, false for inactive only, null for all.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>List of session entities.</returns>
    Task<List<SessionEntity>> GetUserSessionsAsync(
        Guid userId,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets all sessions with pagination and filtering (for admin).
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="status">Filter by status: "active", "expired", or null for all.</param>
    /// <param name="userId">Optional filter by user ID.</param>
    /// <param name="ipAddress">Optional filter by IP address (partial match).</param>
    /// <param name="deviceName">Optional filter by device name (partial match).</param>
    /// <param name="fromDate">Optional filter for sessions created after this date.</param>
    /// <param name="toDate">Optional filter for sessions created before this date.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Tuple of sessions list and total count.</returns>
    Task<(List<SessionEntity> sessions, int totalCount)> GetAllWithPaginationAsync(
        int page,
        int pageSize,
        string? status = null,
        Guid? userId = null,
        string? ipAddress = null,
        string? deviceName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets count of active sessions grouped by client platform (from Client-Platform header).
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Dictionary with the client platform as key and count as value.</returns>
    Task<Dictionary<string, int>> GetActiveSessionCountByClientPlatformAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets count of active sessions grouped by device type (classified from user agent).
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Dictionary with the device type as the key and count as value.</returns>
    Task<Dictionary<string, int>> GetActiveSessionCountByDeviceTypeAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets total count of active sessions.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Total count of active sessions.</returns>
    Task<int> GetTotalActiveSessionsCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of unique active users.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Total count of unique users with active sessions.</returns>
    Task<int> GetTotalActiveUsersCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sessions for export with optional filtering.
    /// </summary>
    /// <param name="status">Filter by status: "active", "expired", or null for all.</param>
    /// <param name="fromDate">Optional filter for sessions created after this date.</param>
    /// <param name="toDate">Optional filter for sessions created before this date.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>List of session entities for export.</returns>
    Task<List<SessionEntity>> GetSessionsForExportAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes all expired sessions (soft delete for cleanup).
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of sessions deleted.</returns>
    Task<int> DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the refresh token for a session (for token rotation).
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="newRefreshTokenHash">The new hashed refresh token.</param>
    /// <param name="newExpiresAt">The new expiration date.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task UpdateRefreshTokenAsync(
        Guid sessionId,
        string newRefreshTokenHash,
        DateTime newExpiresAt,
        CancellationToken cancellationToken = default
    );
}
