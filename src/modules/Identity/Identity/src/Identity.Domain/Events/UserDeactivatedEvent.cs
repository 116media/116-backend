using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when an account transitions from active to inactive. Consumers revoke the account's
/// live sessions so a deactivated user is signed out everywhere, not just refused at next login.
/// </summary>
/// <param name="UserId">The deactivated account.</param>
public record UserDeactivatedEvent(Guid UserId) : DomainEvent;
