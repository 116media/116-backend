using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when an account transitions from inactive to active. Raised only on the actual
/// transition, so re-activating an active account stays silent.
/// </summary>
/// <param name="UserId">The reactivated account.</param>
public record UserActivatedEvent(Guid UserId) : DomainEvent;
