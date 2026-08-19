using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a role is granted to a user through the administrative
/// assignment flow. Bootstrap assignments (signup visitor role, seeders) do
/// not raise it: they are same-transaction invariants, not notifiable facts.
/// </summary>
/// <param name="UserId">The user who received the role.</param>
/// <param name="RoleId">The granted role.</param>
/// <param name="RoleName">The granted role's name, captured at grant time.</param>
public record UserRoleGrantedEvent(Guid UserId, Guid RoleId, string RoleName) : IDomainEvent;
