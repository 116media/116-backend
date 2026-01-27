using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Factories;

namespace _116.Unit.Tests.Common.Mothers;

/// <summary>
/// Provides predefined <see cref="PermissionEntity"/> scenarios for testing.
/// </summary>
public static class PermissionMother
{
    /// <summary>
    /// A read permission for users resource.
    /// </summary>
    public static PermissionEntity UsersRead() =>
        new PermissionBuilder()
            .WithResourceAction("users", "read")
            .WithDescription("Allows reading user information from the system.")
            .Build();

    /// <summary>
    /// A create permission for users resource.
    /// </summary>
    public static PermissionEntity UsersCreate() =>
        new PermissionBuilder()
            .WithResourceAction("users", "create")
            .WithDescription("Allows creating new user accounts.")
            .Build();

    /// <summary>
    /// An update permission for users resource.
    /// </summary>
    public static PermissionEntity UsersUpdate() =>
        new PermissionBuilder()
            .WithResourceAction("users", "update")
            .WithDescription("Allows updating existing user information.")
            .Build();

    /// <summary>
    /// A delete permission for users resource.
    /// </summary>
    public static PermissionEntity UsersDelete() =>
        new PermissionBuilder()
            .WithResourceAction("users", "delete")
            .WithDescription("Allows deleting user accounts.")
            .Build();

    /// <summary>
    /// A read permission for articles resource.
    /// </summary>
    public static PermissionEntity ArticlesRead() =>
        new PermissionBuilder()
            .WithResourceAction("articles", "read")
            .WithDescription("Allows reading article content.")
            .Build();

    /// <summary>
    /// A create permission for articles resource.
    /// </summary>
    public static PermissionEntity ArticlesCreate() =>
        new PermissionBuilder()
            .WithResourceAction("articles", "create")
            .WithDescription("Allows creating new articles.")
            .Build();

    /// <summary>
    /// An approve permission for articles resource.
    /// </summary>
    public static PermissionEntity ArticlesApprove() =>
        new PermissionBuilder()
            .WithResourceAction("articles", "approve")
            .WithDescription("Allows approving article submissions.")
            .Build();

    /// <summary>
    /// A permission that has been deactivated.
    /// </summary>
    public static PermissionEntity Inactive() =>
        new PermissionBuilder()
            .WithResourceAction("system", "read")
            .WithDescription("A permission that has been deactivated.")
            .AsInactive()
            .Build();

    /// <summary>
    /// A permission that has been soft-deleted.
    /// </summary>
    public static PermissionEntity Deleted() =>
        new PermissionBuilder()
            .WithResourceAction("legacy", "access")
            .WithDescription("A permission that has been soft-deleted.")
            .AsDeleted()
            .Build();

    /// <summary>
    /// A permission with valid test data.
    /// </summary>
    public static PermissionEntity Valid() =>
        new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .WithDescription(TestConstants.Permission.ValidDescription)
            .Build();

    /// <summary>
    /// A random permission for general testing.
    /// </summary>
    public static PermissionEntity Random() => new PermissionBuilder().Build();

    /// <summary>
    /// Full CRUD permissions for users resource.
    /// </summary>
    public static List<PermissionEntity> UsersCrud() => PermissionFactory.CreateCrud("users");

    /// <summary>
    /// Full CRUD permissions for articles resource.
    /// </summary>
    public static List<PermissionEntity> ArticlesCrud() => PermissionFactory.CreateCrud("articles");

    /// <summary>
    /// Common visitor permissions.
    /// </summary>
    public static List<PermissionEntity> VisitorPermissions() =>
        [
            ArticlesRead(),
            new PermissionBuilder()
                .WithResourceAction("own_profile", "read")
                .WithDescription("Allows reading own profile.")
                .Build(),
            new PermissionBuilder()
                .WithResourceAction("own_profile", "update")
                .WithDescription("Allows updating own profile.")
                .Build(),
        ];
}
