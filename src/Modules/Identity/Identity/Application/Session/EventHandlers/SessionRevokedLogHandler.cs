using _116.Identity.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Session.EventHandlers;

/// <summary>
/// Logs every session revocation with its cause. This is the audit-ready slot for the revocation
/// fact: future consumers (token denylist, audit trail, push notifications) subscribe to the same
/// event; v1 only records it.
/// </summary>
/// <param name="logger">Logger recording the revocation fact.</param>
public class SessionRevokedLogHandler(ILogger<SessionRevokedLogHandler> logger)
    : IDomainEventHandler<SessionRevokedEvent>
{
    /// <inheritdoc />
    public Task Handle(SessionRevokedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Session {SessionId} of user {UserId} revoked: {Reason}.",
            domainEvent.SessionId,
            domainEvent.UserId,
            domainEvent.Reason
        );

        return Task.CompletedTask;
    }
}
