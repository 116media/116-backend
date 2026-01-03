using _116.Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the RolePermissionEntity.
/// Defines database mapping, relationships, and constraints for role-permission associations.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
    /// <summary>
    /// Configures the RolePermissionEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the RolePermissionEntity</param>
    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        // Primary key
        builder.HasKey(rp => rp.Id);

        // Properties configuration
        builder.Property(rp => rp.RoleId)
            .IsRequired();
        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        // Indexes
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique()
            .HasDatabaseName("IX_role_permissions_role_id_permission_id");

        // Relationships are configured in RoleConfiguration and PermissionConfiguration
        // to avoid circular dependencies
    }
}
