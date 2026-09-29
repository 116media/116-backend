using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Member entity of the User aggregate recording a single role grant.
/// Created and removed only through <see cref="UserEntity" />'s grant/revoke methods.
/// </summary>
public class UserRoleEntity : Entity<Guid>
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
    /// Creates a user-role association. The grant fact is raised by the user aggregate, not
    /// here — a member entity carries no events. The key stays unset so change detection
    /// tracks a row reached through the user's collection as an insert.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A new <see cref="UserRoleEntity" /> instance.</returns>
    internal static UserRoleEntity Create(Guid userId, Guid roleId)
    {
        return new UserRoleEntity { UserId = userId, RoleId = roleId };
    }
}
