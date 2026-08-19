using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead;

/// <summary>
/// Handles the <see cref="PublicMarkAllNotificationsReadCommand" />: loads the
/// requesting user's unread rows, marks each read with a single shared
/// timestamp, and commits once. A call with nothing unread commits nothing.
/// </summary>
/// <param name="notificationRepository">Repository for notification persistence.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
public class PublicMarkAllNotificationsReadHandler(
    INotificationRepository notificationRepository,
    IMailerUnitOfWork unitOfWork
) : ICommandHandler<PublicMarkAllNotificationsReadCommand, PublicMarkAllNotificationsReadResult>
{
    /// <summary>
    /// Handles the bulk mark-read transition.
    /// </summary>
    public async Task<PublicMarkAllNotificationsReadResult> Handle(
        PublicMarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<NotificationEntity> unread = await notificationRepository.GetUnreadForUserAsync(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        if (unread.Count == 0)
        {
            return new PublicMarkAllNotificationsReadResult(MarkedCount: 0);
        }

        DateTime now = DateTime.UtcNow;

        foreach (NotificationEntity notification in unread)
        {
            notification.MarkRead(now);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return new PublicMarkAllNotificationsReadResult(MarkedCount: unread.Count);
    }
}
