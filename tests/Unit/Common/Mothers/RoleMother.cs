using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Mothers;

/// <summary>
/// Provides predefined <see cref="RoleEntity"/> scenarios for testing.
/// </summary>
public static class RoleMother
{
    /// <summary>
    /// A SuperAdmin role with full system access.
    /// </summary>
    public static RoleEntity SuperAdmin() =>
        new RoleBuilder()
            .WithName(TestConstants.Role.SuperAdminName)
            .WithDescription(TestConstants.Role.SuperAdminDescription)
            .Build();

    /// <summary>
    /// A SuperAdmin role with a specific ID.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    public static RoleEntity SuperAdmin(Guid id) =>
        new RoleBuilder()
            .WithId(id)
            .WithName(TestConstants.Role.SuperAdminName)
            .WithDescription(TestConstants.Role.SuperAdminDescription)
            .Build();

    /// <summary>
    /// An Admin role with administrative access.
    /// </summary>
    public static RoleEntity Admin() =>
        new RoleBuilder()
            .WithName(TestConstants.Role.AdminName)
            .WithDescription(TestConstants.Role.AdminDescription)
            .Build();

    /// <summary>
    /// An Admin role with a specific ID.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    public static RoleEntity Admin(Guid id) =>
        new RoleBuilder()
            .WithId(id)
            .WithName(TestConstants.Role.AdminName)
            .WithDescription(TestConstants.Role.AdminDescription)
            .Build();

    /// <summary>
    /// A Visitor role with standard user access.
    /// </summary>
    public static RoleEntity Visitor() =>
        new RoleBuilder()
            .WithName(TestConstants.Role.VisitorName)
            .WithDescription(TestConstants.Role.VisitorDescription)
            .Build();

    /// <summary>
    /// A Visitor role with a specific ID.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    public static RoleEntity Visitor(Guid id) =>
        new RoleBuilder()
            .WithId(id)
            .WithName(TestConstants.Role.VisitorName)
            .WithDescription(TestConstants.Role.VisitorDescription)
            .Build();

    /// <summary>
    /// A role that has been deactivated.
    /// </summary>
    public static RoleEntity Inactive() =>
        new RoleBuilder()
            .WithName("InactiveRole")
            .WithDescription("A role that has been deactivated and cannot be assigned.")
            .AsInactive()
            .Build();

    /// <summary>
    /// A role that has been soft-deleted.
    /// </summary>
    public static RoleEntity Deleted() =>
        new RoleBuilder()
            .WithName("DeletedRole")
            .WithDescription("A role that has been soft-deleted.")
            .AsDeleted()
            .Build();

    /// <summary>
    /// A role with valid test data.
    /// </summary>
    public static RoleEntity Valid() =>
        new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .Build();

    /// <summary>
    /// A random role for general testing.
    /// </summary>
    public static RoleEntity Random() => new RoleBuilder().Build();

    /// <summary>
    /// All core roles (SuperAdmin, Admin, Visitor).
    /// </summary>
    public static List<RoleEntity> CoreRoles() => [SuperAdmin(), Admin(), Visitor()];
}
