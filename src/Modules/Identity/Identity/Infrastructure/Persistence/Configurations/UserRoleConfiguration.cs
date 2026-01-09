using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the UserRoleEntity.
/// Defines database mapping, relationships, and constraints for user-role associations.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    /// <summary>
    /// Configures the UserRoleEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the UserRoleEntity</param>
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        // Primary key
        builder.HasKey(ur => ur.Id);

        // Properties configuration
        builder.Property(ur => ur.UserId).IsRequired();
        builder.Property(ur => ur.RoleId).IsRequired();

        // Indexes
        builder
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("IX_user_roles_user_id_role_id");

        // Relationships are configured in UserConfiguration and RoleConfiguration
        // to avoid circular dependencies
    }
}
