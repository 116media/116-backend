using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead;

/// <summary>
/// Handles the <see cref="PublicMarkNotificationReadCommand" />: resolves the
/// row scoped to its owner and marks it read. Re-marks are no-ops that keep
/// the original read time; a row that does not exist for this user resolves
/// to the same not-found error as an unknown id, so existence never leaks.
/// </summary>
/// <param name="notificationRepository">Repository for notification persistence.</param>
/// <param name="unitOfWork">The Mailer module unit of work.</param>
/// <param name="errors">Notification error factory for missing rows.</param>
public class PublicMarkNotificationReadHandler(
    INotificationRepository notificationRepository,
    IMailerUnitOfWork unitOfWork,
    NotificationErrors errors
) : ICommandHandler<PublicMarkNotificationReadCommand, PublicMarkNotificationReadResult>
{
    /// <summary>
    /// Handles the mark-read transition and commits on the first mark only.
    /// </summary>
    public async Task<PublicMarkNotificationReadResult> Handle(
        PublicMarkNotificationReadCommand command,
        CancellationToken cancellationToken
    )
    {
        NotificationEntity notification =
            await notificationRepository.GetForUserAsync(command.NotificationId, command.UserId, cancellationToken)
            ?? throw errors.NotificationNotFound();

        bool marked = notification.MarkRead(now: DateTime.UtcNow);

        if (marked)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return new PublicMarkNotificationReadResult(IsRead: true);
    }
}
