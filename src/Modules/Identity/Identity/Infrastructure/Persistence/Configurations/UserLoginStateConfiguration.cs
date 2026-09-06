using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the UserLoginStateEntity.
/// Defines the 1:1 mapping between a user and their login brute-force counters.
/// </summary>
public class UserLoginStateConfiguration : IEntityTypeConfiguration<UserLoginStateEntity>
{
    /// <summary>
    /// Configures the UserLoginStateEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the UserLoginStateEntity.</param>
    public void Configure(EntityTypeBuilder<UserLoginStateEntity> builder)
    {
        builder.ToTable(name: "user_login_state");

        // Primary key doubles as the FK to the owning user (1:1)
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName(name: "user_id");

        builder.Property(s => s.FailedAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.LockedUntil).IsRequired(false);

        builder
            .HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<UserLoginStateEntity>(s => s.Id)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
    }
}
