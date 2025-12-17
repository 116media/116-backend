using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using _116.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace _116.Auth.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Manages database operations for Super Admin seeding.
/// Implements Repository pattern to encapsulate data access logic.
/// </summary>
public class SuperAdminRepositoryManager(AuthDbContext context, ILogger<SuperAdminRepositoryManager> logger)
{
    /// <summary>
    /// Checks if a Super Admin user already exists in the database.
    /// </summary>
    /// <returns>True if Super Admin exists, false otherwise.</returns>
    public async Task<bool> SuperAdminExistsAsync()
    {
        UserEntity? existingSuperAdmin = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == SuperAdminConfiguration.Email);

        return existingSuperAdmin != null;
    }

    /// <summary>
    /// Finds an existing permission by resource and action.
    /// </summary>
    /// <param name="resource">The permission resource.</param>
    /// <param name="action">The permission action.</param>
    /// <returns>The existing permission or null if not found.</returns>
    public async Task<PermissionEntity?> FindPermissionAsync(string resource, string action)
    {
        return await context.Permissions
            .FirstOrDefaultAsync(p => p.Resource == resource && p.Action == action);
    }

    /// <summary>
    /// Finds an existing role by name.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>The existing role or null if not found.</returns>
    public async Task<RoleEntity?> FindRoleAsync(string roleName)
    {
        return await context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    /// <summary>
    /// Checks if a role-permission association already exists.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID.</param>
    /// <returns>True if association exists, false otherwise.</returns>
    public async Task<bool> RolePermissionExistsAsync(Guid roleId, Guid permissionId)
    {
        return await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    /// <summary>
    /// Checks if a user-role association already exists.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleId">The role ID.</param>
    /// <returns>True if association exists, false otherwise.</returns>
    public async Task<bool> UserRoleExistsAsync(Guid userId, Guid roleId)
    {
        return await context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    /// <summary>
    /// Adds a permission entity to the context.
    /// </summary>
    /// <param name="permission">The permission to add.</param>
    public void AddPermission(PermissionEntity permission)
    {
        context.Permissions.Add(permission);
        logger.LogDebug("Added permission: {Resource}.{Action}", permission.Resource, permission.Action);
    }

    /// <summary>
    /// Adds a role entity to the context.
    /// </summary>
    /// <param name="role">The role to add.</param>
    public void AddRole(RoleEntity role)
    {
        context.Roles.Add(role);
        logger.LogDebug("Added role: {RoleName}", role.Name);
    }

    /// <summary>
    /// Adds a user entity to the context.
    /// </summary>
    /// <param name="user">The user to add.</param>
    public void AddUser(UserEntity user)
    {
        context.Users.Add(user);
        logger.LogDebug("Added user: {Username}", user.UserName);
    }

    /// <summary>
    /// Adds a role-permission association to the context.
    /// </summary>
    /// <param name="rolePermission">The role-permission association to add.</param>
    public void AddRolePermission(RolePermissionEntity rolePermission)
    {
        context.RolePermissions.Add(rolePermission);
        logger.LogDebug(
            "Added role-permission association: RoleId={RoleId}, PermissionId={PermissionId}",
            rolePermission.RoleId, rolePermission.PermissionId
        );
    }

    /// <summary>
    /// Adds a user-role association to the context.
    /// </summary>
    /// <param name="userRole">The user-role association to add.</param>
    public void AddUserRole(UserRoleEntity userRole)
    {
        context.UserRoles.Add(userRole);
        logger.LogDebug(
            "Added user-role association: UserId={UserId}, RoleId={RoleId}",
            userRole.UserId, userRole.RoleId
        );
    }

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
        logger.LogDebug("Saved changes to database");
    }

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    /// <returns>The database transaction.</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        logger.LogDebug("Started database transaction");
        return transaction;
    }
}
