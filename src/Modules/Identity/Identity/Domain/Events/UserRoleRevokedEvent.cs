using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a role is revoked from a user through the administrative
/// removal flow.
/// </summary>
/// <param name="UserId">The user who lost the role.</param>
/// <param name="RoleId">The revoked role.</param>
/// <param name="RoleName">The revoked role's name, captured at revocation time.</param>
public record UserRoleRevokedEvent(Guid UserId, Guid RoleId, string RoleName) : IDomainEvent;
