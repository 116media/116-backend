using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Factory class for creating Super Admin related entities.
/// Implements the factory pattern to encapsulate entity creation logic.
/// </summary>
public class SuperAdminEntityFactory(IPasswordService passwordService)
{
    /// <summary>
    /// Creates a Super Admin user entity with proper configuration.
    /// </summary>
    /// <returns>A configured <see cref="UserEntity" /> for the Super Admin.</returns>
    public UserEntity CreateSuperAdminUser()
    {
        string password = SuperAdminConfiguration.GetPassword();
        string hashedPassword = passwordService.Hash(password: password);
        var superAdminUser = UserEntity.Create(
            Guid.NewGuid(),
            email: SuperAdminConfiguration.Email,
            userName: SuperAdminConfiguration.Username,
            passwordHash: hashedPassword
        );
        // Configure Super Admin as verified and active
        // IsLoggedIn remains false as per specification
        superAdminUser.MarkAsVerified();
        superAdminUser.Activate();
        return superAdminUser;
    }

    /// <summary>
    /// Creates the Super Admin role entity.
    /// </summary>
    /// <returns>A configured <see cref="RoleEntity" /> for Super Admin.</returns>
    public static RoleEntity CreateSuperAdminRole()
    {
        return RoleEntity.Create(
            Guid.NewGuid(),
            name: SuperAdminConfiguration.RoleName,
            description: SuperAdminConfiguration.RoleDescription
        );
    }

    /// <summary>
    /// Creates the system-wide "system.all" permission entity.
    /// </summary>
    /// <returns>A configured <see cref="PermissionEntity" /> for system-wide access.</returns>
    public static PermissionEntity CreateSystemAllPermission()
    {
        return PermissionEntity.Create(
            Guid.NewGuid(),
            resource: SuperAdminConfiguration.PermissionResource,
            action: SuperAdminConfiguration.PermissionAction,
            description: SuperAdminConfiguration.PermissionDescription
        );
    }

    /// <summary>
    /// Creates a user-role association entity.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleId">The role ID.</param>
    /// <returns>A <see cref="UserRoleEntity" /> linking the user and role.</returns>
    public static UserRoleEntity CreateUserRoleAssociation(Guid userId, Guid roleId)
    {
        return UserRoleEntity.Create(Guid.NewGuid(), userId: userId, roleId: roleId);
    }

    /// <summary>
    /// Creates a role-permission association entity.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID.</param>
    /// <returns>A <see cref="RolePermissionEntity" /> linking the role and permission.</returns>
    public static RolePermissionEntity CreateRolePermissionAssociation(Guid roleId, Guid permissionId)
    {
        return RolePermissionEntity.Create(Guid.NewGuid(), roleId: roleId, permissionId: permissionId);
    }
}
