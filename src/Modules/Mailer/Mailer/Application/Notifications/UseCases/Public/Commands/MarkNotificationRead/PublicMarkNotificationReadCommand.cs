using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead;

/// <summary>
/// Command for marking one of the authenticated user's notifications read.
/// </summary>
/// <param name="UserId">The requesting user, resolved from the JWT.</param>
/// <param name="NotificationId">The notification to mark read.</param>
/// <remarks>
/// Idempotent: marking an already read notification succeeds and keeps the
/// original read time. A notification that does not exist for this user —
/// unknown id or another user's row alike — resolves to a 404 problem.
/// </remarks>
public record PublicMarkNotificationReadCommand(Guid UserId, Guid NotificationId)
    : ICommand<PublicMarkNotificationReadResult>;

/// <summary>
/// Result of the <see cref="PublicMarkNotificationReadCommand" />.
/// </summary>
/// <param name="IsRead">Whether the notification is read after the call.</param>
public record PublicMarkNotificationReadResult(bool IsRead);
