using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Builders.Entities;

namespace _116.Identity.TestData.Factories;

/// <summary>
/// Named aliases for <see cref="UserRoleBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
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
    /// Creates a user-role association with a specific ID.
    /// </summary>
    /// <param name="id">The association identifier.</param>
    /// <returns>A new UserRoleEntity with the specified ID.</returns>
    public static UserRoleEntity CreateWithId(Guid id) => new UserRoleBuilder().WithId(id).Build();

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
