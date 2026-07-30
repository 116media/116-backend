using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a presented refresh token matches a session that was already
/// revoked: someone is replaying a credential that was deliberately
/// invalidated. Consumers revoke the account's remaining sessions and alert
/// the owner; the refresh attempt itself is still rejected.
/// </summary>
/// <param name="UserId">The user whose revoked token was replayed.</param>
/// <param name="SessionId">The revoked session the replayed token belonged to.</param>
public record RefreshTokenReplayDetectedEvent(Guid UserId, Guid SessionId) : IDomainEvent;
