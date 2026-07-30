using _116.Identity.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a user's password hash is replaced, whatever the flow. The
/// origin selects the security email template. Session revocation is not a
/// consumer of this event: it is part of the same transaction as the password
/// change, so the payload carries no session routing data.
/// </summary>
/// <param name="UserId">The user whose password changed.</param>
/// <param name="Origin">The flow that replaced the password.</param>
public record UserPasswordChangedEvent(Guid UserId, EnumPasswordChangeOrigin Origin) : IDomainEvent;
