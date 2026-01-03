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
        builder.Property(s => s.UserId)
            .IsRequired();
        builder.Property(s => s.RefreshTokenHash)
            .HasMaxLength(maxLength: SessionConstants.MaxRefreshTokenHashLength)
            .IsRequired();
        builder.Property(s => s.CreatedAt)
            .IsRequired();
        builder.Property(s => s.ExpiresAt)
            .IsRequired();
        builder.Property(s => s.IpAddress)
            .HasMaxLength(maxLength: SessionConstants.MaxIpAddressLength)
            .IsRequired(false);
        builder.Property(s => s.UserAgent)
            .HasMaxLength(maxLength: SessionConstants.MaxUserAgentLength)
            .IsRequired(false);
        builder.Property(s => s.DeviceName)
            .HasMaxLength(maxLength: SessionConstants.MaxDeviceNameLength)
            .IsRequired(false);
        builder.Property(s => s.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();
        builder.Property(s => s.DeletedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.RefreshTokenHash);
        builder.HasIndex(s => s.ExpiresAt);
        builder.HasIndex(s => s.IsDeleted);

        // Relationships configured in UserConfiguration
    }
}
