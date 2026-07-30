using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a user's account email address changes. The addresses are part
/// of the fact itself: the alert email must reach the old address and the
/// confirmation the new one. Session revocation is not a consumer of this
/// event: it is part of the same transaction as the email change, so the
/// payload carries no session routing data.
/// </summary>
/// <param name="UserId">The user whose email changed.</param>
/// <param name="OldEmail">The address the account had before the change, when one existed.</param>
/// <param name="NewEmail">The address the account holds after the change.</param>
public record UserEmailChangedEvent(Guid UserId, string? OldEmail, string NewEmail) : IDomainEvent;
