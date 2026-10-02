using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Events;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Access behaviour of <see cref="UserEntity" />. Its state lives in <c>Entities/UserEntity.cs</c>.
/// </summary>
public sealed partial class UserEntity
{
    /// <summary>
    /// Activates the account so the user can log in again, raising
    /// <see cref="UserActivatedEvent" />. Idempotent: an active account reports <c>false</c>
    /// and raises nothing.
    /// </summary>
    /// <returns><c>true</c> if the account transitioned; <c>false</c> if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = UserConstants.ActivatedStatus;

        AddDomainEvent(new UserActivatedEvent(UserId: Id));
        return true;
    }

    /// <summary>
    /// Deactivates the account so the user can no longer log in, raising
    /// <see cref="UserDeactivatedEvent" />; consumers revoke the account's live sessions.
    /// Idempotent: an inactive account reports <c>false</c> and raises nothing.
    /// </summary>
    /// <returns><c>true</c> if the account transitioned; <c>false</c> if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = UserConstants.DeactivatedStatus;

        AddDomainEvent(new UserDeactivatedEvent(UserId: Id));
        return true;
    }

    // Role Methods
    /// <summary>
    /// Grants a role to this user and raises <see cref="UserRoleGrantedEvent" /> carrying the
    /// role name. Idempotent: a role already granted reports <c>false</c> and raises nothing.
    /// </summary>
    /// <param name="roleId">The ID of the role to grant.</param>
    /// <param name="roleName">The granted role's name, carried by the event.</param>
    /// <returns><c>true</c> if the role was granted; <c>false</c> if already granted.</returns>
    public bool GrantRole(Guid roleId, string roleName)
    {
        if (!GrantInitialRole(roleId: roleId))
        {
            return false;
        }

        AddDomainEvent(new UserRoleGrantedEvent(UserId: Id, RoleId: roleId, RoleName: roleName));
        return true;
    }

    /// <summary>
    /// Grants a role as part of creating the account, raising no event: the visitor grant on
    /// signup is a same-transaction invariant, not a fact worth notifying the new user about.
    /// Idempotent: a role already granted reports <c>false</c>.
    /// </summary>
    /// <param name="roleId">The ID of the role to grant.</param>
    /// <returns><c>true</c> if the role was granted; <c>false</c> if already granted.</returns>
    public bool GrantInitialRole(Guid roleId)
    {
        if (HasRole(roleId: roleId))
        {
            return false;
        }

        UserRoles.Add(item: UserRoleEntity.Create(userId: Id, roleId: roleId));
        return true;
    }

    /// <summary>
    /// Revokes a role from this user and raises <see cref="UserRoleRevokedEvent" /> carrying the
    /// role name. Idempotent: a role not granted reports <c>false</c> and raises nothing.
    /// </summary>
    /// <param name="roleId">The ID of the role to revoke.</param>
    /// <param name="roleName">The revoked role's name, carried by the event.</param>
    /// <returns><c>true</c> if the role was revoked; <c>false</c> if it was not granted.</returns>
    public bool RevokeRole(Guid roleId, string roleName)
    {
        UserRoleEntity? userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole is null)
        {
            return false;
        }

        UserRoles.Remove(item: userRole);

        AddDomainEvent(new UserRoleRevokedEvent(UserId: Id, RoleId: roleId, RoleName: roleName));
        return true;
    }

    /// <summary>
    /// Checks if this user has a specific role.
    /// </summary>
    public bool HasRole(Guid roleId)
    {
        return UserRoles.Any(ur => ur.RoleId == roleId);
    }
}
