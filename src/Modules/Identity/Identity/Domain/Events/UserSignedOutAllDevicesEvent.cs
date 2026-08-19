using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when every session on a user's account is terminated at once,
/// either by the user (sign-out from all devices) or by an administrator
/// (force-logout). The user aggregate owns the fact because the session
/// family has no single aggregate to raise from.
/// </summary>
/// <param name="UserId">The user whose sessions were terminated.</param>
/// <param name="ByAdmin">Whether an administrator drove the termination.</param>
public record UserSignedOutAllDevicesEvent(Guid UserId, bool ByAdmin) : IDomainEvent;
