using _116.Identity.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a session is revoked, whatever the cause. This is the single
/// hook for revocation consumers (token denylist, audit trail, push
/// notifications); only an audit log consumes it in v1.
/// </summary>
/// <param name="UserId">The user who owned the session.</param>
/// <param name="SessionId">The revoked session.</param>
/// <param name="Reason">Why the session was revoked.</param>
public record SessionRevokedEvent(Guid UserId, Guid SessionId, EnumSessionRevokeReason Reason) : IDomainEvent;
