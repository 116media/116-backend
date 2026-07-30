using _116.Identity.Domain.Events;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents the many-to-many association between users and roles.
/// </summary>
public class UserRoleEntity : Aggregate<Guid>
{
    /// <summary>
    /// Foreign key referencing the associated user.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Foreign key referencing the associated role.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Navigation property for the associated user.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    /// <summary>
    /// Navigation property for the associated role.
    /// </summary>
    public RoleEntity Role { get; private set; } = null!;

    /// <summary>
    /// Creates a user-role association for the administrative grant flow and raises
    /// <see cref="UserRoleGrantedEvent" /> with the role name captured at grant time.
    /// </summary>
    /// <param name="id">The unique identifier of the association.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="roleId">The ID of the role.</param>
    /// <param name="roleName">The granted role's name, carried by the event.</param>
    /// <returns>A new <see cref="UserRoleEntity" /> instance.</returns>
    public static UserRoleEntity Create(Guid id, Guid userId, Guid roleId, string roleName)
    {
        UserRoleEntity userRole = CreateBootstrap(id: id, userId: userId, roleId: roleId);

        userRole.AddDomainEvent(new UserRoleGrantedEvent(UserId: userId, RoleId: roleId, RoleName: roleName));

        return userRole;
    }

    /// <summary>
    /// Creates a user-role association without raising the grant event. This is the bootstrap
    /// path — the signup visitor assignment and the seeders — where the association is a
    /// same-transaction invariant rather than a notifiable fact.
    /// </summary>
    /// <param name="id">The unique identifier of the association.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A new <see cref="UserRoleEntity" /> instance.</returns>
    public static UserRoleEntity CreateBootstrap(Guid id, Guid userId, Guid roleId)
    {
        return new UserRoleEntity
        {
            Id = id,
            UserId = userId,
            RoleId = roleId,
        };
    }

    /// <summary>
    /// Records that this association is being removed through the administrative revocation flow
    /// by raising <see cref="UserRoleRevokedEvent" />. The caller deletes the row in the same
    /// transaction; the event carries the role name so consumers never re-fetch the role.
    /// </summary>
    /// <param name="roleName">The revoked role's name, captured at revocation time.</param>
    public void RecordRevocation(string roleName)
    {
        AddDomainEvent(new UserRoleRevokedEvent(UserId: UserId, RoleId: RoleId, RoleName: roleName));
    }
}
