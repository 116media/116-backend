using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Session.Cache;
using _116.Identity.Domain.Events;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Session.EventHandlers;

/// <summary>
/// Reacts to every session revocation: adds the session to the in-process denylist and logs the
/// revocation with its cause.
/// </summary>
/// <param name="revocationCache">Denylist rejecting the revoked session's still-live tokens.</param>
/// <param name="logger">Logger recording the revocation fact.</param>
public class SessionRevokedLogHandler(ISessionRevocationCache revocationCache, ILogger<SessionRevokedLogHandler> logger)
    : IDomainEventHandler<SessionRevokedEvent>
{
    /// <inheritdoc />
    public Task Handle(SessionRevokedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // The entry only needs to outlive the access tokens minted for the session.
        var (_, _, _, accessTokenExpiration, _) = AppEnvironment.Jwt();
        int ttlMinutes = int.TryParse(s: accessTokenExpiration, out int parsed)
            ? parsed
            : JwtClaimsConstants.DefaultExpiration;

        revocationCache.Revoke(sessionId: domainEvent.SessionId, ttl: TimeSpan.FromMinutes(ttlMinutes));

        logger.LogInformation(
            "Session {SessionId} of user {UserId} revoked: {Reason}.",
            domainEvent.SessionId,
            domainEvent.UserId,
            domainEvent.Reason
        );

        return Task.CompletedTask;
    }
}
