using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a login reuses an existing session row for a known device,
/// rotating its refresh token instead of inserting a duplicate. No consumer
/// is registered in v1.
/// </summary>
/// <param name="SessionId">The reactivated session.</param>
/// <param name="UserId">The user the session belongs to.</param>
public record SessionReactivatedEvent(Guid SessionId, Guid UserId) : IDomainEvent;
