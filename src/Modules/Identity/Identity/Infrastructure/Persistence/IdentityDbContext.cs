using System.Reflection;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the auth module.
/// Manages user entities, roles, permissions, and their relationships within the "authentication" schema.
/// </summary>
/// <param name="options">The database context configuration options</param>
/// <remarks>
/// This context provides access to user authentication data including user accounts, roles,
/// permissions, and their many-to-many relationships. All entities are stored in the "authentication" schema.
/// </remarks>
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options: options)
{
    /// <summary>
    /// Gets the collection of user entities representing user accounts in the system.
    /// </summary>
    /// <value>DbSet of UserEntity for managing user account data</value>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>
    /// Gets the collection of role entities representing user roles in the system.
    /// </summary>
    /// <value>DbSet of RoleEntity for managing role definitions</value>
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();

    /// <summary>
    /// Gets the collection of permission entities representing system permissions.
    /// </summary>
    /// <value>DbSet of PermissionEntity for managing permission definitions</value>
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();

    /// <summary>
    /// Gets the collection of user-role associations representing the
    /// many-to-many relationship between users and roles.
    /// </summary>
    /// <value>DbSet of UserRoleEntity for managing user-role assignments</value>
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();

    /// <summary>
    /// Gets the collection of role-permission associations representing
    /// the many-to-many relationship between roles and permissions.
    /// </summary>
    /// <value>DbSet of RolePermissionEntity for managing role-permission assignments</value>
    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();

    /// <summary>
    /// Gets the collection of OTP entities representing one-time passwords for user verification.
    /// </summary>
    /// <value>DbSet of OtpEntity for managing OTP codes and verification</value>
    public DbSet<OtpEntity> Otps => Set<OtpEntity>();

    /// <summary>
    /// Gets the collection of session entities representing active user login sessions.
    /// </summary>
    /// <value>DbSet of SessionEntity for managing user sessions</value>
    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();

    /// <summary>
    /// Configures the model for the context using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <remarks>
    /// Sets the default schema to "authentication" and applies
    /// all entity configurations from the current assembly.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(schema: IdentityConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder: modelBuilder);
    }
}
