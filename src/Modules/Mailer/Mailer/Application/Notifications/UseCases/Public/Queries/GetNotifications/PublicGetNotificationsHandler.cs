using _116.Mailer.Application.Shared.DTOs;
using _116.Mailer.Application.Shared.Mappers;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetNotifications;

/// <summary>
/// Handles the <see cref="PublicGetNotificationsQuery" /> by paging the
/// requesting user's notifications, newest first, with an optional
/// unread-only filter. Ownership is enforced by the user-scoped repository
/// read — no other user's rows are reachable.
/// </summary>
/// <param name="notificationRepository">Repository for notification persistence.</param>
public class PublicGetNotificationsHandler(INotificationRepository notificationRepository)
    : IQueryHandler<PublicGetNotificationsQuery, PublicGetNotificationsResult>
{
    /// <summary>
    /// Handles the paginated feed listing.
    /// </summary>
    public async Task<PublicGetNotificationsResult> Handle(
        PublicGetNotificationsQuery query,
        CancellationToken cancellationToken
    )
    {
        PaginatedResult<NotificationEntity> page = await notificationRepository.GetPagedForUserAsync(
            userId: query.UserId,
            pageIndex: query.PageIndex,
            pageSize: query.PageSize,
            unreadOnly: query.UnreadOnly,
            cancellationToken: cancellationToken
        );

        var notifications = new PaginatedResult<NotificationDto>(
            pageIndex: page.PageIndex,
            pageSize: page.PageSize,
            count: page.Count,
            items: page.Items.ToNotificationDtoList()
        );

        return new PublicGetNotificationsResult(Notifications: notifications);
    }
}
