using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities;

namespace _116.Tests.Fixtures.Factories;

/// <summary>
/// Factory for quickly creating <see cref="PermissionEntity"/> instances in tests.
/// </summary>
public static class PermissionFactory
{
    /// <summary>
    /// Creates a permission with default random values.
    /// </summary>
    /// <returns>A new PermissionEntity with random values.</returns>
    public static PermissionEntity Create() => new PermissionBuilder().Build();

    /// <summary>
    /// Creates a permission with a specific resource and action.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <param name="action">The action name.</param>
    /// <returns>A new PermissionEntity with the specified values.</returns>
    public static PermissionEntity Create(string resource, string action) =>
        new PermissionBuilder().WithResourceAction(resource, action).Build();

    /// <summary>
    /// Creates a permission with a specific resource, action, and description.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <param name="action">The action name.</param>
    /// <param name="description">The permission description.</param>
    /// <returns>A new PermissionEntity with the specified values.</returns>
    public static PermissionEntity Create(string resource, string action, string description) =>
        new PermissionBuilder().WithResourceAction(resource, action).WithDescription(description).Build();

    /// <summary>
    /// Creates a permission with a specific ID.
    /// </summary>
    /// <param name="id">The permission identifier.</param>
    /// <returns>A new PermissionEntity with the specified ID.</returns>
    public static PermissionEntity CreateWithId(Guid id) => new PermissionBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a permission with a specific ID, resource, and action.
    /// </summary>
    /// <param name="id">The permission identifier.</param>
    /// <param name="resource">The resource name.</param>
    /// <param name="action">The action name.</param>
    /// <returns>A new PermissionEntity with the specified values.</returns>
    public static PermissionEntity CreateWithId(Guid id, string resource, string action) =>
        new PermissionBuilder().WithId(id).WithResourceAction(resource, action).Build();

    /// <summary>
    /// Creates an inactive permission.
    /// </summary>
    /// <returns>A new inactive PermissionEntity.</returns>
    public static PermissionEntity CreateInactive() => new PermissionBuilder().AsInactive().Build();

    /// <summary>
    /// Creates a soft-deleted permission.
    /// </summary>
    /// <returns>A new soft-deleted PermissionEntity.</returns>
    public static PermissionEntity CreateDeleted() => new PermissionBuilder().AsDeleted().Build();

    /// <summary>
    /// Creates a list of permissions with the specified count.
    /// </summary>
    /// <param name="count">The number of permissions to create.</param>
    /// <returns>A list of PermissionEntity instances.</returns>
    public static List<PermissionEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>
    /// Creates a read permission for a specific resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>A new read PermissionEntity.</returns>
    public static PermissionEntity CreateRead(string resource) => Create(resource, PermissionActions.Read);

    /// <summary>
    /// Creates a create-permission for a specific resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>A new creation PermissionEntity.</returns>
    public static PermissionEntity CreateCreate(string resource) => Create(resource, PermissionActions.Create);

    /// <summary>
    /// Creates an update permission for a specific resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>A new update PermissionEntity.</returns>
    public static PermissionEntity CreateUpdate(string resource) => Create(resource, PermissionActions.Update);

    /// <summary>
    /// Creates a delete permission for a specific resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>A new delete PermissionEntity.</returns>
    public static PermissionEntity CreateDelete(string resource) => Create(resource, PermissionActions.Delete);

    /// <summary>
    /// Creates CRUD permissions for a specific resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>A list of CRUD PermissionEntity instances.</returns>
    public static List<PermissionEntity> CreateCrud(string resource) =>
        [CreateRead(resource), CreateCreate(resource), CreateUpdate(resource), CreateDelete(resource)];

    /// <summary>
    /// Standard permission actions.
    /// </summary>
    public static class PermissionActions
    {
        public const string Read = "read";
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Approve = "approve";
    }
}
