using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkAllNotificationsRead;

/// <summary>
/// Command for marking every unread notification of the authenticated user read.
/// </summary>
/// <param name="UserId">The requesting user, resolved from the JWT.</param>
/// <remarks>
/// Idempotent: a second call finds nothing unread and marks zero rows.
/// </remarks>
public record PublicMarkAllNotificationsReadCommand(Guid UserId) : ICommand<PublicMarkAllNotificationsReadResult>;

/// <summary>
/// Result of the <see cref="PublicMarkAllNotificationsReadCommand" />.
/// </summary>
/// <param name="MarkedCount">The number of notifications transitioned to read by this call.</param>
public record PublicMarkAllNotificationsReadResult(int MarkedCount);
