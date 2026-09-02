using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the UserOtpStateEntity.
/// Defines the 1:1 mapping with the user (the primary key is the user id) and the counter columns.
/// </summary>
public class UserOtpStateConfiguration : IEntityTypeConfiguration<UserOtpStateEntity>
{
    /// <summary>
    /// Configures the UserOtpStateEntity mapping and its 1:1 relationship with the user.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the UserOtpStateEntity</param>
    public void Configure(EntityTypeBuilder<UserOtpStateEntity> builder)
    {
        builder.ToTable(name: "user_otp_state");

        // Primary key doubles as the FK to the owning user (1:1)
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName(name: "user_id");

        // Properties configuration
        builder.Property(s => s.FailedAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.LockedUntil).IsRequired(false);

        // Relationships
        builder
            .HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<UserOtpStateEntity>(s => s.Id)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
    }
}
