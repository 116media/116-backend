using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Queries.GetUnreadNotificationCount;

/// <summary>
/// Query for the authenticated user's unread notification count.
/// </summary>
/// <param name="UserId">The requesting user, resolved from the JWT.</param>
public record PublicGetUnreadNotificationCountQuery(Guid UserId) : IQuery<PublicGetUnreadNotificationCountResult>;

/// <summary>
/// Result of the <see cref="PublicGetUnreadNotificationCountQuery" />.
/// </summary>
/// <param name="Count">The number of unread notifications.</param>
public record PublicGetUnreadNotificationCountResult(int Count);
