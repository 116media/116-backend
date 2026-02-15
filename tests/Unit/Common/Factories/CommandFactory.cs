using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;
using _116.Unit.Tests.Common.Builders.Commands.Roles;

namespace _116.Unit.Tests.Common.Factories;

/// <summary>
/// Factory for quickly creating command instances in tests.
/// </summary>
public static class CommandFactory
{
    /// <summary>
    /// Role command factories.
    /// </summary>
    public static class Role
    {
        /// <summary>
        /// Creates a create role command with default random values.
        /// </summary>
        /// <returns>A new AdminCreateRoleCommand with random values.</returns>
        public static AdminCreateRoleCommand CreateCommand() => new CreateRoleCommandBuilder().Build();

        /// <summary>
        /// Creates a create role command with valid test data.
        /// </summary>
        /// <returns>A new AdminCreateRoleCommand with valid test values.</returns>
        public static AdminCreateRoleCommand CreateValidCommand() =>
            new CreateRoleCommandBuilder().WithValidData().Build();

        /// <summary>
        /// Creates a create role command with a specific name.
        /// </summary>
        /// <param name="name">The role name.</param>
        /// <returns>A new AdminCreateRoleCommand with the specified name.</returns>
        public static AdminCreateRoleCommand CreateCommand(string name) =>
            new CreateRoleCommandBuilder().WithName(name).Build();

        /// <summary>
        /// Creates a create role command with a specific name and description.
        /// </summary>
        /// <param name="name">The role name.</param>
        /// <param name="description">The role description.</param>
        /// <returns>A new AdminCreateRoleCommand with the specified values.</returns>
        public static AdminCreateRoleCommand CreateCommand(string name, string description) =>
            new CreateRoleCommandBuilder().WithName(name).WithDescription(description).Build();

        /// <summary>
        /// Creates an update role command with default random values.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns>A new AdminUpdateRoleCommand with random values.</returns>
        public static AdminUpdateRoleCommand UpdateCommand(Guid roleId) =>
            new UpdateRoleCommandBuilder().WithRoleId(roleId).Build();

        /// <summary>
        /// Creates an update role command with valid test data.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns>A new AdminUpdateRoleCommand with valid test values.</returns>
        public static AdminUpdateRoleCommand UpdateValidCommand(Guid roleId) =>
            new UpdateRoleCommandBuilder().WithRoleId(roleId).WithValidData().Build();

        /// <summary>
        /// Creates an update role command with specific values.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="name">The new role name.</param>
        /// <param name="description">The new role description.</param>
        /// <returns>A new AdminUpdateRoleCommand with the specified values.</returns>
        public static AdminUpdateRoleCommand UpdateCommand(Guid roleId, string? name, string? description) =>
            new UpdateRoleCommandBuilder().WithRoleId(roleId).WithName(name).WithDescription(description).Build();
    }

    /// <summary>
    /// Permission command factories.
    /// </summary>
    public static class Permission
    {
        /// <summary>
        /// Creates a create permission command with default random values.
        /// </summary>
        /// <returns>A new AdminCreatePermissionCommand with random values.</returns>
        public static AdminCreatePermissionCommand CreateCommand() => new CreatePermissionCommandBuilder().Build();

        /// <summary>
        /// Creates a create permission command with valid test data.
        /// </summary>
        /// <returns>A new AdminCreatePermissionCommand with valid test values.</returns>
        public static AdminCreatePermissionCommand CreateValidCommand() =>
            new CreatePermissionCommandBuilder().WithValidData().Build();

        /// <summary>
        /// Creates a create permission command with specific values.
        /// </summary>
        /// <param name="resource">The resource name.</param>
        /// <param name="action">The action name.</param>
        /// <returns>A new AdminCreatePermissionCommand with the specified values.</returns>
        public static AdminCreatePermissionCommand CreateCommand(string resource, string action) =>
            new CreatePermissionCommandBuilder().WithResource(resource).WithAction(action).Build();

        /// <summary>
        /// Creates a create permission command with specific values.
        /// </summary>
        /// <param name="resource">The resource name.</param>
        /// <param name="action">The action name.</param>
        /// <param name="description">The permission description.</param>
        /// <returns>A new AdminCreatePermissionCommand with the specified values.</returns>
        public static AdminCreatePermissionCommand CreateCommand(string resource, string action, string description) =>
            new CreatePermissionCommandBuilder()
                .WithResource(resource)
                .WithAction(action)
                .WithDescription(description)
                .Build();

        /// <summary>
        /// Creates an update permission command with default random values.
        /// </summary>
        /// <param name="permissionId">The permission identifier.</param>
        /// <returns>A new AdminUpdatePermissionCommand with random values.</returns>
        public static AdminUpdatePermissionCommand UpdateCommand(Guid permissionId) =>
            new UpdatePermissionCommandBuilder().WithPermissionId(permissionId).Build();

        /// <summary>
        /// Creates an update permission command with valid test data.
        /// </summary>
        /// <param name="permissionId">The permission identifier.</param>
        /// <returns>A new AdminUpdatePermissionCommand with valid test values.</returns>
        public static AdminUpdatePermissionCommand UpdateValidCommand(Guid permissionId) =>
            new UpdatePermissionCommandBuilder().WithPermissionId(permissionId).WithValidData().Build();

        /// <summary>
        /// Creates an update permission command with specific values.
        /// </summary>
        /// <param name="permissionId">The permission identifier.</param>
        /// <param name="resource">The new resource name.</param>
        /// <param name="action">The new action name.</param>
        /// <param name="description">The new description.</param>
        /// <returns>A new AdminUpdatePermissionCommand with the specified values.</returns>
        public static AdminUpdatePermissionCommand UpdateCommand(
            Guid permissionId,
            string? resource,
            string? action,
            string? description
        ) =>
            new UpdatePermissionCommandBuilder()
                .WithPermissionId(permissionId)
                .WithResource(resource)
                .WithAction(action)
                .WithDescription(description)
                .Build();
    }
}
