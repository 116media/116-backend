using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a user's account transitions to verified after a successful
/// email verification. Raised only on the actual transition, so re-verifying
/// an already-verified account never fires it.
/// </summary>
/// <param name="UserId">The verified user.</param>
public record UserVerifiedEvent(Guid UserId) : IDomainEvent;
