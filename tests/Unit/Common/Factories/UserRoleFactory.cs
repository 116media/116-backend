using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;

namespace _116.Unit.Tests.Common.Factories;

/// <summary>
/// Factory for quickly creating <see cref="UserRoleEntity"/> instances in tests.
/// </summary>
public static class UserRoleFactory
{
    /// <summary>
    /// Creates a user-role association with default random values.
    /// </summary>
    /// <returns>A new UserRoleEntity with random values.</returns>
    public static UserRoleEntity Create() => new UserRoleBuilder().Build();

    /// <summary>
    /// Creates a user-role association for a specific user and role.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>A new UserRoleEntity with the specified user and role.</returns>
    public static UserRoleEntity Create(Guid userId, Guid roleId) =>
        new UserRoleBuilder().ForUserAndRole(userId, roleId).Build();

    /// <summary>
    /// Creates a user-role association with the Role navigation property set.
    /// Useful for testing scenarios where the Role relationship needs to be loaded.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role entity to associate.</param>
    /// <returns>A new UserRoleEntity with the Role navigation property set.</returns>
    public static UserRoleEntity CreateWithRole(Guid userId, RoleEntity role) =>
        new UserRoleBuilder().WithUserId(userId).WithRole(role).Build();

    /// <summary>
    /// Creates a user-role association with a specific ID.
    /// </summary>
    /// <param name="id">The association identifier.</param>
    /// <returns>A new UserRoleEntity with the specified ID.</returns>
    public static UserRoleEntity CreateWithId(Guid id) => new UserRoleBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a list of user-role associations with the specified count.
    /// </summary>
    /// <param name="count">The number of associations to create.</param>
    /// <returns>A list of UserRoleEntity instances.</returns>
    public static List<UserRoleEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>
    /// Creates a user-role association with a specific user ID.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new UserRoleEntity with the specified user ID.</returns>
    public static UserRoleEntity CreateWithUserId(Guid userId) => new UserRoleBuilder().WithUserId(userId).Build();

    /// <summary>
    /// Creates a user-role association with a specific role ID.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>A new UserRoleEntity with the specified role ID.</returns>
    public static UserRoleEntity CreateWithRoleId(Guid roleId) => new UserRoleBuilder().WithRoleId(roleId).Build();

    /// <summary>
    /// Creates a user-role association with a specific ID, user ID, and role ID.
    /// </summary>
    /// <param name="id">The association identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <returns>A new UserRoleEntity with the specified values.</returns>
    public static UserRoleEntity CreateWithId(Guid id, Guid userId, Guid roleId) =>
        new UserRoleBuilder().WithId(id).WithUserId(userId).WithRoleId(roleId).Build();
}
