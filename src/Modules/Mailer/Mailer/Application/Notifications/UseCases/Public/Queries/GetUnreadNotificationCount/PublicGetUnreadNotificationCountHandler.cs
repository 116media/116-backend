using _116.Mailer.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount;

/// <summary>
/// Handles the <see cref="PublicGetUnreadNotificationCountQuery" /> by
/// counting the requesting user's rows without a read time — the number the
/// frontend badge displays.
/// </summary>
/// <param name="notificationRepository">Repository for notification persistence.</param>
public class PublicGetUnreadNotificationCountHandler(INotificationRepository notificationRepository)
    : IQueryHandler<PublicGetUnreadNotificationCountQuery, PublicGetUnreadNotificationCountResult>
{
    /// <summary>
    /// Handles the unread count lookup.
    /// </summary>
    public async Task<PublicGetUnreadNotificationCountResult> Handle(
        PublicGetUnreadNotificationCountQuery query,
        CancellationToken cancellationToken
    )
    {
        int count = await notificationRepository.CountUnreadAsync(
            userId: query.UserId,
            cancellationToken: cancellationToken
        );

        return new PublicGetUnreadNotificationCountResult(Count: count);
    }
}
