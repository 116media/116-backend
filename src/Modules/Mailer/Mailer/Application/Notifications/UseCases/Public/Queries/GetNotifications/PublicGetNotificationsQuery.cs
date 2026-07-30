using _116.Mailer.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications;

/// <summary>
/// Query for the authenticated user's paginated notification feed.
/// </summary>
/// <param name="UserId">The requesting user, resolved from the JWT.</param>
/// <param name="PageIndex">The zero-based page index.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="UnreadOnly">Whether to return unread rows only.</param>
public record PublicGetNotificationsQuery(Guid UserId, int PageIndex, int PageSize, bool UnreadOnly)
    : IQuery<PublicGetNotificationsResult>;

/// <summary>
/// Result of the <see cref="PublicGetNotificationsQuery" />.
/// </summary>
/// <param name="Notifications">The paginated notifications, newest first.</param>
public record PublicGetNotificationsResult(PaginatedResult<NotificationDto> Notifications);
