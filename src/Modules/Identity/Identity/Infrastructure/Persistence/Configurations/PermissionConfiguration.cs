using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the PermissionEntity.
/// Defines database mapping, relationships, constraints, and indexes for system permissions.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    /// <summary>
    /// Configures the PermissionEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the PermissionEntity</param>
    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        // Primary key
        builder.HasKey(p => p.Id);
        // Properties configuration
        builder.Property(p => p.Resource)
            .HasMaxLength(maxLength: PermissionConstants.MaxPermissionResourceLength)
            .IsRequired();
        builder.Property(p => p.Action)
            .HasMaxLength(maxLength: PermissionConstants.MaxPermissionActionLength)
            .IsRequired();
        builder.Property(p => p.Description)
            .HasMaxLength(maxLength: PermissionConstants.MaxPermissionDescriptionLength)
            .IsRequired();
        // Indexes
        builder.HasIndex(p => new { p.Resource, p.Action })
            .IsUnique()
            .HasDatabaseName("IX_permissions_resource_action");
        // Relationships
        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
    }
}
