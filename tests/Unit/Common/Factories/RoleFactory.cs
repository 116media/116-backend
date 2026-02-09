using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Factories;

/// <summary>
/// Factory for quickly creating <see cref="RoleEntity"/> instances in tests.
/// </summary>
public static class RoleFactory
{
    /// <summary>
    /// Creates a role with default random values.
    /// </summary>
    /// <returns>A new RoleEntity with random values.</returns>
    public static RoleEntity Create() => new RoleBuilder().Build();

    /// <summary>
    /// Creates a role with a specific name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>A new RoleEntity with the specified name.</returns>
    public static RoleEntity Create(string name) => new RoleBuilder().WithName(name).Build();

    /// <summary>
    /// Creates a role with a specific name and description.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <returns>A new RoleEntity with the specified values.</returns>
    public static RoleEntity Create(string name, string description) =>
        new RoleBuilder().WithName(name).WithDescription(description).Build();

    /// <summary>
    /// Creates a role with a specific ID.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <returns>A new RoleEntity with the specified ID.</returns>
    public static RoleEntity CreateWithId(Guid id) => new RoleBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a role with a specific ID and name.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="name">The role name.</param>
    /// <returns>A new RoleEntity with the specified values.</returns>
    public static RoleEntity CreateWithId(Guid id, string name) => new RoleBuilder().WithId(id).WithName(name).Build();

    /// <summary>
    /// Creates an inactive role.
    /// </summary>
    /// <returns>A new inactive RoleEntity.</returns>
    public static RoleEntity CreateInactive() => new RoleBuilder().AsInactive().Build();

    /// <summary>
    /// Creates an inactive role with a specific name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>A new inactive RoleEntity with the specified name.</returns>
    public static RoleEntity CreateInactive(string name) => new RoleBuilder().WithName(name).AsInactive().Build();

    /// <summary>
    /// Creates a soft-deleted role.
    /// </summary>
    /// <returns>A new soft-deleted RoleEntity.</returns>
    public static RoleEntity CreateDeleted() => new RoleBuilder().AsDeleted().Build();

    /// <summary>
    /// Creates a soft-deleted role with a specific name.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <returns>A new soft-deleted RoleEntity with the specified name.</returns>
    public static RoleEntity CreateDeleted(string name) => new RoleBuilder().WithName(name).AsDeleted().Build();

    /// <summary>
    /// Creates a list of roles with the specified count.
    /// </summary>
    /// <param name="count">The number of roles to create.</param>
    /// <returns>A list of RoleEntity instances.</returns>
    public static List<RoleEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>
    /// Creates a SuperAdmin role.
    /// </summary>
    /// <returns>A RoleEntity representing SuperAdmin.</returns>
    public static RoleEntity CreateSuperAdmin() =>
        new RoleBuilder()
            .WithName(TestConstants.Role.SuperAdminName)
            .WithDescription(TestConstants.Role.SuperAdminDescription)
            .Build();

    /// <summary>
    /// Creates an Admin role.
    /// </summary>
    /// <returns>A RoleEntity representing Admin.</returns>
    public static RoleEntity CreateAdmin() =>
        new RoleBuilder()
            .WithName(TestConstants.Role.AdminName)
            .WithDescription(TestConstants.Role.AdminDescription)
            .Build();

    /// <summary>
    /// Creates a Visitor role.
    /// </summary>
    /// <returns>A RoleEntity representing Visitor.</returns>
    public static RoleEntity CreateVisitor() =>
        new RoleBuilder()
            .WithName(TestConstants.Role.VisitorName)
            .WithDescription(TestConstants.Role.VisitorDescription)
            .Build();
}
