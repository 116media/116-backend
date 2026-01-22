using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the RoleEntity.
/// Defines database mapping, relationships, constraints, and indexes for user roles.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    /// <summary>
    /// Configures the RoleEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the RoleEntity</param>
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        // Primary key
        builder.HasKey(r => r.Id);

        // Properties configuration
        builder.Property(r => r.Name).HasMaxLength(maxLength: RoleConstants.MaxRoleNameLength).IsRequired();
        builder
            .Property(r => r.Description)
            .HasMaxLength(maxLength: RoleConstants.MaxRoleDescriptionLength)
            .IsRequired();
        builder.Property(r => r.IsActive).HasDefaultValue(RoleConstants.DefaultIsActive).IsRequired();
        builder.Property(r => r.IsDeleted).HasDefaultValue(RoleConstants.DefaultIsDeleted).IsRequired();
        builder.Property(r => r.DeletedAt).IsRequired(false);

        // Indexes
        builder.HasIndex(r => r.Name).IsUnique();
        builder.HasIndex(r => r.IsActive).HasDatabaseName("IX_roles_is_active");
        builder.HasIndex(r => r.IsDeleted).HasDatabaseName("IX_roles_is_deleted");

        // Relationships
        builder
            .HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        builder
            .HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
    }
}
