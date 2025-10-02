using Microsoft.Extensions.Logging;
using _116.Auth.Domain.Entities;

namespace _116.Auth.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Implements the seeding strategy for Super Admin setup.
/// Uses the Strategy pattern to encapsulate the seeding algorithm.
/// </summary>
public class SuperAdminSeedingStrategy(
    SuperAdminEntityFactory entityFactory,
    SuperAdminRepositoryManager repositoryManager,
    ILogger<SuperAdminSeedingStrategy> logger
)
{
    /// <summary>
    /// Executes the Super Admin seeding strategy.
    /// </summary>
    /// <returns>A task representing the asynchronous seeding operation.</returns>
    public async Task ExecuteSeedingAsync()
    {
        logger.LogInformation("Executing Super Admin seeding strategy...");

        // Step 1: Create or get the system-wide permission
        PermissionEntity systemPermission = await CreateOrGetSystemPermissionAsync();
        logger.LogInformation("System permission ready: {Resource}.{Action}",
            systemPermission.Resource, systemPermission.Action);

        // Step 2: Create or get the Super Admin role
        RoleEntity superAdminRole = await CreateOrGetSuperAdminRoleAsync();
        logger.LogInformation("Super Admin role ready: {RoleName}", superAdminRole.Name);

        // Step 3: Associate permission with the role
        await AssociatePermissionWithRoleAsync(superAdminRole.Id, systemPermission.Id);
        logger.LogInformation("Associated system permission with Super Admin role");

        // Step 4: Create the Super Admin user
        UserEntity superAdminUser = CreateSuperAdminUser();
        repositoryManager.AddUser(superAdminUser);
        logger.LogInformation("Created Super Admin user: {Username}", superAdminUser.UserName);

        // Step 5: Associate user with the role
        await AssociateUserWithRoleAsync(superAdminUser.Id, superAdminRole.Id);
        logger.LogInformation("Associated Super Admin user with role");

        logger.LogInformation("Super Admin seeding strategy completed successfully");
    }

    /// <summary>
    /// Creates or retrieves the existing system-wide permission.
    /// </summary>
    private async Task<PermissionEntity> CreateOrGetSystemPermissionAsync()
    {
        PermissionEntity? existingPermission = await repositoryManager.FindPermissionAsync(
            SuperAdminConfiguration.PermissionResource,
            SuperAdminConfiguration.PermissionAction
        );

        if (existingPermission != null)
        {
            logger.LogDebug("Found existing system permission");
            return existingPermission;
        }

        PermissionEntity systemPermission = SuperAdminEntityFactory.CreateSystemAllPermission();
        repositoryManager.AddPermission(systemPermission);
        logger.LogDebug("Created new system permission");

        return systemPermission;
    }

    /// <summary>
    /// Creates or retrieves the existing Super Admin role.
    /// </summary>
    private async Task<RoleEntity> CreateOrGetSuperAdminRoleAsync()
    {
        RoleEntity? existingRole = await repositoryManager.FindRoleAsync(SuperAdminConfiguration.RoleName);

        if (existingRole != null)
        {
            logger.LogDebug("Found existing Super Admin role");
            return existingRole;
        }

        RoleEntity superAdminRole = SuperAdminEntityFactory.CreateSuperAdminRole();
        repositoryManager.AddRole(superAdminRole);
        logger.LogDebug("Created new Super Admin role");

        return superAdminRole;
    }

    /// <summary>
    /// Creates the Super Admin user entity.
    /// </summary>
    private UserEntity CreateSuperAdminUser() => entityFactory.CreateSuperAdminUser();

    /// <summary>
    /// Associates a permission with a role if the association doesn't already exist.
    /// </summary>
    private async Task AssociatePermissionWithRoleAsync(Guid roleId, Guid permissionId)
    {
        bool exists = await repositoryManager.RolePermissionExistsAsync(roleId, permissionId);

        if (!exists)
        {
            RolePermissionEntity association = SuperAdminEntityFactory.CreateRolePermissionAssociation(roleId, permissionId);
            repositoryManager.AddRolePermission(association);
            logger.LogDebug("Created role-permission association");
        }
        else
        {
            logger.LogDebug("Role-permission association already exists");
        }
    }

    /// <summary>
    /// Associates a user with a role if the association doesn't already exist.
    /// </summary>
    private async Task AssociateUserWithRoleAsync(Guid userId, Guid roleId)
    {
        bool exists = await repositoryManager.UserRoleExistsAsync(userId, roleId);

        if (!exists)
        {
            UserRoleEntity association = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);
            repositoryManager.AddUserRole(association);
            logger.LogDebug("Created user-role association");
        }
        else
        {
            logger.LogDebug("User-role association already exists");
        }
    }
}
