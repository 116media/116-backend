using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the OtpEntity.
/// Defines database mapping, relationships, constraints, and indexes for OTP verification.
/// </summary>
public class OtpConfiguration : IEntityTypeConfiguration<OtpEntity>
{
    /// <summary>
    /// Configures the OtpEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the OtpEntity</param>
    public void Configure(EntityTypeBuilder<OtpEntity> builder)
    {
        // Primary key
        builder.HasKey(o => o.Id);

        // Properties configuration
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Code).HasMaxLength(maxLength: UserConstants.OtpCodeLength).IsRequired();
        builder.Property(o => o.Purpose).HasConversion<string>().IsRequired();
        builder.Property(o => o.ExpiresAt).IsRequired();
        builder.Property(o => o.AttemptCount).HasDefaultValue(0).IsRequired();
        builder.Property(o => o.IsUsed).HasDefaultValue(false).IsRequired();
        builder.Property(o => o.UsedAt).IsRequired(false);
        // Relationships
        builder
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(o => new { o.UserId, o.Purpose }).HasDatabaseName("IX_Otps_UserId_Purpose");
        builder
            .HasIndex(o => new
            {
                o.UserId,
                o.Code,
                o.Purpose,
            })
            .HasDatabaseName("IX_Otps_UserId_Code_Purpose");
        builder.HasIndex(o => o.ExpiresAt).HasDatabaseName("IX_Otps_ExpiresAt");
        builder.HasIndex(o => new { o.Purpose, o.ExpiresAt }).HasDatabaseName("IX_Otps_Purpose_ExpiresAt");
    }
}
