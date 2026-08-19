using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a login creates a brand-new session row. The new-device flag
/// is computed where the reuse-or-create decision is made, giving the
/// deferred login-alert consumer its seam without recomputation. No consumer
/// is registered in v1.
/// </summary>
/// <param name="SessionId">The created session.</param>
/// <param name="UserId">The user the session belongs to.</param>
/// <param name="IsNewDevice">Whether the device had no prior session row for this user.</param>
public record SessionCreatedEvent(Guid SessionId, Guid UserId, bool IsNewDevice) : IDomainEvent;
