using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the SessionEntity.
/// Defines database mapping, relationships, constraints, and indexes for user sessions.
/// </summary>
public class SessionConfiguration : IEntityTypeConfiguration<SessionEntity>
{
    /// <summary>
    /// Configures the SessionEntity mapping and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the SessionEntity</param>
    public void Configure(EntityTypeBuilder<SessionEntity> builder)
    {
        // Primary key
        builder.HasKey(s => s.Id);

        // Properties configuration
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.DeviceId).HasMaxLength(maxLength: SessionConstants.MaxDeviceIdLength).IsRequired();
        builder
            .Property(s => s.RefreshTokenHash)
            .HasMaxLength(maxLength: SessionConstants.MaxRefreshTokenHashLength)
            .IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.ExpiresAt).IsRequired();
        builder
            .Property(s => s.IpAddress)
            .HasMaxLength(maxLength: SessionConstants.MaxIpAddressLength)
            .IsRequired(false);
        builder
            .Property(s => s.UserAgent)
            .HasMaxLength(maxLength: SessionConstants.MaxUserAgentLength)
            .IsRequired(false);
        builder.Property(s => s.Browser).HasConversion<string>().IsRequired();
        builder.Property(s => s.Device).HasConversion<string>().IsRequired();
        builder.Property(s => s.Platform).HasConversion<string>().IsRequired();
        builder.Property(s => s.Client).HasConversion<string>().IsRequired();
        builder.Property(s => s.IsRevoked).HasDefaultValue(false).IsRequired();
        builder.Property(s => s.RevokedAt).IsRequired(false);

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.DeviceId);
        builder.HasIndex(s => s.RefreshTokenHash);
        builder.HasIndex(s => s.ExpiresAt);
        builder.HasIndex(s => s.IsRevoked);

        // Unique constraint: One active session per device per user
        builder.HasIndex(s => new { s.UserId, s.DeviceId }).IsUnique().HasFilter("is_revoked = false");

        // Relationships configured in UserConfiguration
    }
}
