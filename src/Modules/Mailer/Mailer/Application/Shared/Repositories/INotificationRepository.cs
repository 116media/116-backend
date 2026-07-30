using _116.Mailer.Domain.Entities;
using _116.Shared.Application.Pagination;

namespace _116.Mailer.Application.Shared.Repositories;

/// <summary>
/// Persistence port for in-app notifications. Every read is scoped by user id,
/// so ownership isolation is structural rather than a per-call filter.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Adds a new notification to the current unit of work.
    /// </summary>
    /// <param name="notification">The unread notification to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken);

    /// <summary>
    /// Pages a user's notifications, newest first.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="unreadOnly">Whether to return unread rows only.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The requested page with the total count of matching rows.</returns>
    Task<PaginatedResult<NotificationEntity>> GetPagedForUserAsync(
        Guid userId,
        int pageIndex,
        int pageSize,
        bool unreadOnly,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Counts a user's unread notifications.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of rows with no read time.</returns>
    Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single notification by id, scoped to its owner. Returns null for
    /// unknown ids and for rows owned by another user alike, so callers cannot
    /// distinguish the two.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="userId">The owning user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The tracked notification, or null when it does not exist for this user.</returns>
    Task<NotificationEntity?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all of a user's unread notifications, tracked for mutation.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unread notifications.</returns>
    Task<IReadOnlyList<NotificationEntity>> GetUnreadForUserAsync(Guid userId, CancellationToken cancellationToken);
}
