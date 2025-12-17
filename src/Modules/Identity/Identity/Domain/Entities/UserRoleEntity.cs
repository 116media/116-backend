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
    /// Creates a new user-role association.
    /// </summary>
    /// <param name="id">The unique identifier of the association.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A new <see cref="UserRoleEntity"/> instance.</returns>
    public static UserRoleEntity Create(Guid id, Guid userId, Guid roleId)
    {
        return new UserRoleEntity
        {
            Id = id,
            UserId = userId,
            RoleId = roleId
        };
    }
}
